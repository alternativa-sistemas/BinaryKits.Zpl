using System;
using System.Runtime.InteropServices;
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.ElementDrawers;
using RGiesecke.DllExport;

namespace BinaryKits.Zpl.NativeWrapper
{
    public static class ZplInterop
    {
        [DllExport("ConvertZplToPng", CallingConvention = CallingConvention.StdCall)]
        public static int ConvertZplToPng(IntPtr zplDataPtr, IntPtr outputBufferPtr, IntPtr bufferSizePtr, int widthMm, int heightMm, int dpmm)
        {
            try
            {
                if (zplDataPtr == IntPtr.Zero || bufferSizePtr == IntPtr.Zero)
                {
                    return -1; // Invalid arguments
                }

                string zplData = Marshal.PtrToStringAnsi(zplDataPtr);
                if (string.IsNullOrEmpty(zplData))
                {
                    return -1; // Empty ZPL
                }

                IPrinterStorage printerStorage = new PrinterStorage();
                var drawerOptions = new DrawerOptions
                {
                    OpaqueBackground = true
                };

                var drawer = new ZplElementDrawer(printerStorage, drawerOptions);
                var analyzer = new ZplAnalyzer(printerStorage);
                var analyzeInfo = analyzer.Analyze(zplData);

                if (analyzeInfo.LabelInfos.Length == 0)
                {
                    return -2; // No labels found
                }

                // Render the first label
                var labelInfo = analyzeInfo.LabelInfos[0];
                var imageData = drawer.Draw(labelInfo.ZplElements, widthMm, heightMm, dpmm);

                if (imageData == null || imageData.Length == 0)
                {
                    return -3; // Rendering failed
                }

                int requiredSize = imageData.Length;
                int currentSize = Marshal.ReadInt32(bufferSizePtr);

                // Always write back the required size
                Marshal.WriteInt32(bufferSizePtr, requiredSize);

                if (currentSize < requiredSize)
                {
                    return 1; // Buffer too small
                }

                if (outputBufferPtr != IntPtr.Zero)
                {
                    Marshal.Copy(imageData, 0, outputBufferPtr, requiredSize);
                }

                return 0; // Success
            }
            catch (Exception)
            {
                return -99; // Exception
            }
        }
    }
}
