program TestZpl;

{$APPTYPE CONSOLE}

uses
  SysUtils,
  Classes,
  Math;

// Declare the function from the DLL
function ConvertZplToPng(
  zplData: PAnsiChar;
  outputBuffer: PByte;
  bufferSize: PInteger;
  widthMm: Integer;
  heightMm: Integer;
  dpmm: Integer
): Integer; stdcall; external 'BinaryKits.Zpl.NativeWrapper.dll';

procedure Main;
var
  zpl: AnsiString;
  res, size: Integer;
  buffer: TBytes;
  fs: TFileStream;
  savedMask: TArithmeticExceptionMask;
begin
  try
    Writeln('Starting ZPL to PNG conversion test...');

    // Sample ZPL
    zpl := '^XA^FO50,50^ADN,36,20^FDHello Delphi^FS^XZ';
    size := 0;

    Writeln('1. Getting required buffer size...');
    
    // Mask ALL arithmetic exceptions (FPU + SSE)
    // .NET and Skia use SSE instructions which can trigger exceptions if not masked
    savedMask := SetExceptionMask(exAllArithmeticExceptions);
    try
      // 1. Get required size (pass nil for buffer)
      res := ConvertZplToPng(PAnsiChar(zpl), nil, @size, 100, 100, 8);
    finally
      SetExceptionMask(savedMask);
    end;

    Writeln(Format('   Result: %d, Required Size: %d', [res, size]));

    if (res = 1) and (size > 0) then
    begin
      Writeln('2. Allocating buffer and converting...');
      // 2. Allocate buffer
      SetLength(buffer, size);

      // Call again with buffer (masking again)
      savedMask := SetExceptionMask(exAllArithmeticExceptions);
      try
        res := ConvertZplToPng(PAnsiChar(zpl), @buffer[0], @size, 100, 100, 8);
      finally
        SetExceptionMask(savedMask);
      end;
      
      Writeln(Format('   Result: %d', [res]));

      if res = 0 then
      begin
        Writeln('3. Saving to delphi_test.png...');
        fs := TFileStream.Create('delphi_test.png', fmCreate);
        try
          fs.WriteBuffer(buffer[0], size);
        finally
          fs.Free;
        end;
        Writeln('Success! Check delphi_test.png');
      end
      else
      begin
        Writeln('Error converting ZPL.');
      end;
    end
    else
    begin
      Writeln('Error getting buffer size or empty result.');
    end;

  except
    on E: Exception do
      Writeln(E.ClassName, ': ', E.Message);
  end;
  
  Writeln('Press Enter to exit...');
  Readln;
end;

begin
  Main;
end.
