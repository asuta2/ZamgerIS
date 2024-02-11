namespace ooadproject.Sheets
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
                spreadsheetId = segments[segments.Length - 2];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return spreadsheetId;
        }

        public static Dictionary<int, double> GetExamResults(string link)
        {
            string id;
            try
            {
                id = ExtractSpreadsheetId(link);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            _manager = new GoogleSheetsManager(id);

            var stringResults = _manager.GetExam();

            var results = new Dictionary<int, double>();

            foreach (var kvp in stringResults)
            {
                if (int.TryParse(kvp.Key, out int parsedKey) && double.TryParse(kvp.Value, out double parsedValue))
                {
                    results.Add(parsedKey, parsedValue);
                }
                else
                {
                    throw new Exception("Greška u parsiranju");
                }
            }

            return results;
        }
    }
}
