using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;

public class GoogleSheetsManager
{
    static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    static string ApplicationName = "Zam";
    static string ClientSecretFilePath = "Sheets\\cred.json";
    static string Range = "Sheet1!A1:Z100";
    string SpreadsheetId;

    public GoogleSheetsManager(string _spreadsheetId)
    {
        SpreadsheetId = _spreadsheetId;
    }

    public Dictionary<string, string> GetExam()
    {
        UserCredential credential;

        using (var stream = new FileStream(ClientSecretFilePath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("tokens.json")).Result;
        }

        // Create Google Sheets API service.
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Retrieve the values from the specified range.
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(SpreadsheetId, Range);
        ValueRange? response = request.Execute();
        IList<IList<object>>? values = response?.Values;
        Dictionary<string, string> exam = new Dictionary<string, string>();
        for (int i = 1; values != null && i < values.Count; i++)
        {
            int k = 0;
            string index = "null", points = "null";
            for (int j = 0; values[i] != null && j < values[i].Count; j++)
            {
                if (values[i][j]?.ToString()?.Length != 0)
                {
                    if (k == 0)
                    {
                        index = values[i][j].ToString();
                        k = 1;
                    }
                    points = values[i][j].ToString();
                }
            }
            exam[index] = points;
        }
        return exam;
    }
}