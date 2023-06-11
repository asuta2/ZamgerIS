﻿namespace ooadproject.Sheets
{
    public class SheetsFacade
    {
        private static GoogleSheetsManager _manager;

        public SheetsFacade()
        {

        }

        public static string ExtractSpreadsheetId(string link)
        {
            // Extract the spreadsheet ID from the link
            string spreadsheetId = string.Empty;
            try
            {
                Uri uri = new Uri(link);
                string path = uri.AbsolutePath;
                string[] segments = path.Split('/');
                spreadsheetId = segments[segments.Length - 1];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting spreadsheet ID: " + ex.Message);
            }

            return spreadsheetId;
        }

        public static Dictionary<int, double> GetExamResults(string link)
        {
            string id = ExtractSpreadsheetId(link);

            _manager = new GoogleSheetsManager(id);
            
            var stringResults =  _manager.GetExam();
            
            var results = new Dictionary<int, double>();

           
                foreach (var kvp in stringResults)
                {
                    if (int.TryParse(kvp.Key, out int parsedKey) && double.TryParse(kvp.Value, out double parsedValue))
                    {
                        results.Add(parsedKey, parsedValue);
                    }
                    else
                    {
                        // Handle parsing errors if necessary
                        Console.WriteLine($"Greška u formatu rezultata");
                    }
                }


             
            return results;
        }

    }
}
