namespace ShowExchangeRateBot
{
    public class Logger : LogBase
    {
        private string _currentDirectory { get; set; }
        private string _fileName { get; set; }
        private string _filePath { get; set; }

        public Logger()
        {
            _currentDirectory = Directory.GetCurrentDirectory();
            _fileName = "Log.txt";
            _filePath = $@"{_currentDirectory}/{_fileName}";
        }

        public override void Log(string message)
        {
            using (StreamWriter w = File.AppendText(_filePath))
            {
                w.WriteLine(message);
            }
        }
    }
}
