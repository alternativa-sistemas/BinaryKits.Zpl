using System.Runtime.InteropServices;

namespace TestConsole;

class Program
{
    [DllImport(@"c:\alternativa\BinaryKits.Zpl\src\BinaryKits.Zpl.NativeWrapper\bin\Release\net8.0\win-x64\publish\BinaryKits.Zpl.NativeWrapper.dll", EntryPoint = "ConvertZplToPng", CallingConvention = CallingConvention.StdCall)]
    public static extern int ConvertZplToPng(IntPtr zplDataPtr, IntPtr outputBufferPtr, IntPtr bufferSizePtr, int widthMm, int heightMm, int dpmm);

    static void Main(string[] args)
    {
        string zpl = "^XA^FO50,50^ADN,36,20^FDHello World^FS^XZ";
        IntPtr zplPtr = Marshal.StringToHGlobalAnsi(zpl);
        
        try
        {
            // 1. Get required size
            int bufferSize = 0;
            IntPtr bufferSizePtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(bufferSizePtr, bufferSize);

            int result = ConvertZplToPng(zplPtr, IntPtr.Zero, bufferSizePtr, 100, 100, 8);
            
            bufferSize = Marshal.ReadInt32(bufferSizePtr);
            Console.WriteLine($"First call result: {result}, Required size: {bufferSize}");

            if (result == 1 && bufferSize > 0)
            {
                // 2. Allocate buffer and call again
                IntPtr outputBuffer = Marshal.AllocHGlobal(bufferSize);
                result = ConvertZplToPng(zplPtr, outputBuffer, bufferSizePtr, 100, 100, 8);
                Console.WriteLine($"Second call result: {result}");

                if (result == 0)
                {
                    byte[] data = new byte[bufferSize];
                    Marshal.Copy(outputBuffer, data, 0, bufferSize);
                    File.WriteAllBytes("test.png", data);
                    Console.WriteLine("Saved test.png");
                }
                
                Marshal.FreeHGlobal(outputBuffer);
            }
            
            Marshal.FreeHGlobal(bufferSizePtr);
        }
        finally
        {
            Marshal.FreeHGlobal(zplPtr);
        }
    }
}
