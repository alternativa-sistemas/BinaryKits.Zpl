﻿using BinaryKits.ZplUtility.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace BinaryKits.ZplUtility.UnitTest
{
    [TestClass]
    public class DownloadTest
    {
        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadGraphics.txt")]
        public void DownloadGraphics()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadGraphics('R', "SAMPLE", "GRC", imageData),
                new ZplRecallGraphic(100, 100, 'R', "SAMPLE", "GRC")
            };

            var renderEngine = new ZplEngine(elements);
            var output = renderEngine.ToZplString(new ZplRenderOptions
            {
                AddEmptyLineBeforeElementStart = true,
                TargetPrintDpi = 200,
                SourcePrintDpi = 200
            });

            Debug.WriteLine(output);
            Assert.IsNotNull(output);

            var zplData = File.ReadAllText("DownloadGraphics.txt");
            Assert.AreEqual(zplData, output);
        }

        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadObject.txt")]
        public void DownloadObjects()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            for (var i = 0; i < 100; i++)
            {

                var sw = new Stopwatch();
                sw.Start();

                var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadObjects('R', "SAMPLE.PNG", imageData),
                new ZplImageMove(100, 100, 'R', "SAMPLE", "PNG")
            };

                var renderEngine = new ZplEngine(elements);
                var output = renderEngine.ToZplString(new ZplRenderOptions
                {
                    AddEmptyLineBeforeElementStart = true,
                    TargetPrintDpi = 300,
                    SourcePrintDpi = 200
                });

                sw.Stop();
                Debug.WriteLine(sw.Elapsed.TotalMilliseconds);
            }

            //Debug.WriteLine(output);
            //Assert.IsNotNull(output);

            //var zplData = File.ReadAllText("DownloadObject.txt");
            //Assert.AreEqual(zplData, output);
        }
    }
}
