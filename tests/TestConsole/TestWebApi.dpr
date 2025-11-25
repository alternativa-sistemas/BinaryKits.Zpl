program TestWebApi;

{$APPTYPE CONSOLE}

uses
  System.SysUtils,
  System.Classes,
  ZplApiClient in 'ZplApiClient.pas';

procedure Main;
var
  Client: TZplApiClient;
  PngBytes: TBytes;
  Zpl: string;
begin
  try
    Writeln('Testing Web API Client...');
    
    // Ensure the Web API is running on this URL
    Client := TZplApiClient.Create('http://localhost:5000');
    try
      Zpl := '^XA^FO50,50^ADN,36,20^FDWeb API Test^FS^XZ';
      
      Writeln('Sending Request...');
      PngBytes := Client.ConvertZplToPng(Zpl, 100, 100, 8);
      
      if Length(PngBytes) > 0 then
      begin
        TFile.WriteAllBytes('webapi_test.png', PngBytes);
        Writeln('Success! Saved webapi_test.png');
      end
      else
        Writeln('Error: Empty response.');
        
    except
      on E: Exception do
        Writeln('Error: ' + E.Message);
    end;
    Client.Free;
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
