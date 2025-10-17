using BinaryKits.Zpl.Viewer.ElementDrawers;
using BinaryKits.Zpl.Viewer.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BinaryKits.Zpl.Viewer.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ViewerController : ControllerBase
    {
        private readonly ILogger<ViewerController> _logger;

        public ViewerController(ILogger<ViewerController> logger)
        {
            this._logger = logger;
        }

        [HttpPost]
        public ActionResult<RenderResponseDto> Render(RenderRequestDto request)
        {
            try
            {
                return RenderZpl(request);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("/printers/{dpmm}/labels/{size}/{page}")]
        public async Task<IActionResult> RenderFromPrinter(string dpmm, string size, string page)
        {
            try
            {
                using var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8);
                var content = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return this.BadRequest("Request body must contain ZPL text.");
                }

                // parse dpmm (accept formats like "8dpmm" or "8")
                int dpmmValue;
                if (!TryParseDpmm(dpmm, out dpmmValue))
                {
                    return this.BadRequest("Invalid dpmm format. Use e.g. '8dpmm' or '8'.");
                }

                // parse size (e.g. "4x6" -> width = 4in, height = 6in)
                if (!TryParseSizeInches(size, out double widthIn, out double heightIn))
                {
                    return this.BadRequest("Invalid size format. Use {width}x{height} in inches, e.g. 4x6.");
                }

                int pageonly;
                if (!int.TryParse(page, NumberStyles.Integer, CultureInfo.InvariantCulture, out pageonly))
                {
                    return this.BadRequest("Invalid page number. Use e.g. '0' or '1'.");
                }

                // convert inches to mm
                const double mmPerInch = 25.4;
                var widthMm = (int)Math.Round(widthIn * mmPerInch);
                var heightMm = (int)Math.Round(heightIn * mmPerInch);

                var request = new RenderRequestDto
                {
                    ZplData = content,
                    LabelWidth = widthMm,
                    LabelHeight = heightMm,
                    PrintDensityDpmm = dpmmValue,
                    Type = "image",
                    PageOnly = pageonly
                };

                // Call internal renderer for better performance and return PNG image
                var totalpages = 0;
                var result = RenderZplToPNG(request, out totalpages);
                // Add total pages as response header
                if (result is FileStreamResult || result is FileContentResult || result is FileResult)
                {
                    Response.Headers["X-Total-Count"] = totalpages.ToString();
                }
                return result;
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private static bool TryParseDpmm(string dpmm, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(dpmm)) return false;
            dpmm = dpmm.Trim();
            if (dpmm.EndsWith("dpmm", StringComparison.OrdinalIgnoreCase))
            {
                var numPart = dpmm.Substring(0, dpmm.Length - 4);
                return int.TryParse(numPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            }
            return int.TryParse(dpmm, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseSizeInches(string size, out double widthIn, out double heightIn)
        {
            widthIn = 0; heightIn = 0;
            if (string.IsNullOrWhiteSpace(size)) return false;
            var parts = size.Split(new[] { 'x', 'X' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out widthIn)) return false;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out heightIn)) return false;
            return true;
        }
        private IActionResult RenderZplToPNG(RenderRequestDto request, out int totalpages)
        {
            IPrinterStorage printerStorage = new PrinterStorage();
            var drawerOptions = new DrawerOptions();
            drawerOptions.OpaqueBackground = true; //set white background for viewer requests

            var drawer = new ZplElementDrawer(printerStorage, drawerOptions);

            var analyzer = new ZplAnalyzer(printerStorage);
            var analyzeInfo = analyzer.Analyze(request.ZplData);

            var actualpage = -1;
            totalpages = analyzeInfo.LabelInfos.Length;
            foreach (var labelInfo in analyzeInfo.LabelInfos)
            {
                actualpage++;
                if (labelInfo.ZplElements?.Length <= 0)
                {
                    continue;
                }
                if (request.PageOnly >= 0 && actualpage != request.PageOnly)
                {
                    continue;
                }

                // Draw the label as PNG and return it directly
                var imageData = drawer.Draw(labelInfo.ZplElements, request.LabelWidth, request.LabelHeight, request.PrintDensityDpmm);
                if (imageData != null && imageData.Length > 0)
                {
                    // Return binary PNG with correct content type
                    return File(imageData, "image/png");
                }
            }

            // If no label produced, return 204 No Content
            return this.StatusCode(StatusCodes.Status204NoContent, "No label produced.");
        }


        private ActionResult<RenderResponseDto> RenderZpl(RenderRequestDto request)
        {
            IPrinterStorage printerStorage = new PrinterStorage();
            var drawerOptions = new DrawerOptions();
            drawerOptions.OpaqueBackground = true; //set white background for viewer requests

            //PDF mode (image mode is default)
            if (request.Type == "PDF")
            {
                drawerOptions.PdfOutput = true;
            }

            var drawer = new ZplElementDrawer(printerStorage, drawerOptions);

            var analyzer = new ZplAnalyzer(printerStorage);
            var analyzeInfo = analyzer.Analyze(request.ZplData);

            var labels = new List<RenderLabelDto>();
            var pdfs = new List<RenderLabelDto>();
            var actualpage = -1;
            foreach (var labelInfo in analyzeInfo.LabelInfos)
            {
                actualpage++;
                if (labelInfo.ZplElements?.Length <= 0)
                {
                    continue;
                }
                if (request.PageOnly >= 0 && actualpage != request.PageOnly)
                {
                    continue;
                }
                if (request.Type == "image")
                {
                    var imageData = drawer.Draw(labelInfo.ZplElements, request.LabelWidth, request.LabelHeight, request.PrintDensityDpmm);
                    var label = new RenderLabelDto
                    {
                        ImageBase64 = Convert.ToBase64String(imageData)
                    };
                    labels.Add(label);
                }

                if (request.Type == "PDF")
                {
                    var pdfData = drawer.DrawPdf(labelInfo.ZplElements, request.LabelWidth, request.LabelHeight, request.PrintDensityDpmm);
                    var pdf = new RenderLabelDto
                    {
                        PdfBase64 = Convert.ToBase64String(pdfData)
                    };
                    pdfs.Add(pdf);
                }

                if (request.Type == "both")
                {
                    var bothData = drawer.DrawMulti(labelInfo.ZplElements, request.LabelWidth, request.LabelHeight, request.PrintDensityDpmm);

                    var imageData = bothData[0];
                    var label = new RenderLabelDto
                    {
                        ImageBase64 = Convert.ToBase64String(imageData)
                    };
                    labels.Add(label);

                    var pdfData = bothData[1];
                    var pdf = new RenderLabelDto
                    {
                        PdfBase64 = Convert.ToBase64String(pdfData)
                    };
                    pdfs.Add(pdf);
                }
            }

            var response = new RenderResponseDto
            {
                Labels = labels.ToArray(),
                Pdfs = pdfs.ToArray(),
                NonSupportedCommands = analyzeInfo.UnknownCommands
            };

            return this.StatusCode(StatusCodes.Status200OK, response);
        }
    }    
}
