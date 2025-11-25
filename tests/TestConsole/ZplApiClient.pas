unit ZplApiClient;

interface

uses
  System.SysUtils, System.Classes, System.Net.HttpClient, System.Net.URLClient,
  System.JSON;

type
  EZplApiException = class(Exception);

  TZplApiClient = class
  private
    FBaseUrl: string;
    FClient: THTTPClient;
  public
    constructor Create(const ABaseUrl: string = 'http://localhost:5000');
    destructor Destroy; override;
    
    /// <summary>
    /// Converts ZPL to PNG using the Web API.
    /// </summary>
    /// <param name="AZpl">The ZPL code.</param>
    /// <param name="AWidthMm">Label width in mm (default 100).</param>
    /// <param name="AHeightMm">Label height in mm (default 150).</param>
    /// <param name="ADpmm">Dots per mm (8 = 203dpi, 12 = 300dpi).</param>
    /// <returns>Byte array containing the PNG image.</returns>
    function ConvertZplToPng(const AZpl: string; AWidthMm: Integer = 100; AHeightMm: Integer = 150; ADpmm: Integer = 8): TBytes;
  end;

implementation

{ TZplApiClient }

constructor TZplApiClient.Create(const ABaseUrl: string);
begin
  FBaseUrl := ABaseUrl;
  FClient := THTTPClient.Create;
  FClient.ContentType := 'application/json';
end;

destructor TZplApiClient.Destroy;
begin
  FClient.Free;
  inherited;
end;

function TZplApiClient.ConvertZplToPng(const AZpl: string; AWidthMm, AHeightMm, ADpmm: Integer): TBytes;
var
  LJson: TJSONObject;
  LContent: TStringStream;
  LResponse: IHTTPResponse;
  LUrl: string;
  LResponseStream: TMemoryStream;
begin
  // Prepare JSON payload
  LJson := TJSONObject.Create;
  try
    LJson.AddPair('zplData', AZpl);
    LJson.AddPair('labelWidth', TJSONNumber.Create(AWidthMm));
    LJson.AddPair('labelHeight', TJSONNumber.Create(AHeightMm));
    LJson.AddPair('printDensityDpmm', TJSONNumber.Create(ADpmm));
    LJson.AddPair('type', 'image'); // Request image output
    
    LContent := TStringStream.Create(LJson.ToString, TEncoding.UTF8);
  finally
    LJson.Free;
  end;

  try
    LUrl := Format('%s/api/v1/Viewer', [FBaseUrl]);
    
    // Make POST request
    LResponse := FClient.Post(LUrl, LContent);
    
    if LResponse.StatusCode = 200 then
    begin
      // Parse response to get Base64 image
      // The API returns a JSON object with a "labels" array
      LJson := TJSONObject.ParseJSONValue(LResponse.ContentAsString) as TJSONObject;
      try
        if Assigned(LJson) then
        begin
          var LLabels := LJson.GetValue('labels') as TJSONArray;
          if (Assigned(LLabels)) and (LLabels.Count > 0) then
          begin
            var LLabel := LLabels.Items[0] as TJSONObject;
            var LBase64 := LLabel.GetValue('imageBase64').Value;
            Result := TNetEncoding.Base64.DecodeStringToBytes(LBase64);
          end
          else
            raise EZplApiException.Create('No labels returned from API.');
        end
        else
          raise EZplApiException.Create('Invalid JSON response.');
      finally
        LJson.Free;
      end;
    end
    else
    begin
      raise EZplApiException.CreateFmt('API Error %d: %s', [LResponse.StatusCode, LResponse.StatusText]);
    end;
  finally
    LContent.Free;
  end;
end;

end.
