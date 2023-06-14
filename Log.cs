using System.IO.Compression;
using System.Text;

namespace Logger
{
    /// <summary>
    /// handles log output messages and stores them in a file, this class cannot be inherited
    /// </summary>
    public static class Log //static class so it can never be instantiated
    {
        /// <summary>the directory where the log messages will appear</summary>
        public static string LogDirectory { get; set; }

        /// <summary>used to write to the log file</summary>
        private static StreamWriter LogWriter { get; set; }

        /// <summary>stores the date of the log file to know when it is outdated</summary>
        private static DateTime LogTime { get; set; }

        static Log() //static constructor
        {
            LogDirectory = "./Logs/"; //gets the full path of the default log directory
            NewLog += WriteToFile;

            //if the log directory doesn't exist, create one
            if (Directory.Exists(LogDirectory) == false)
                Directory.CreateDirectory(LogDirectory);

            LogWriter = NewLogWriter(); //create a log writer
        }

        /// <summary>writes to the log file using <see cref="StreamWriter"/> <see cref="LogWriter"/></summary>
        private static void WriteToFile(LogMessage _message)
        {
            //checks if the current log file is outdated
            if (LogTime < DateTime.Now.Date)
            {
                DateTime session = LogTime;

                LogWriter = NewLogWriter();
                Write($"file was outdated! continuing log from {session:dd-MM-yyyy}", "Logger", Severity.Warning);
            }

            //writes to the log file
            LogWriter.WriteLine(_message.Message);
            LogWriter.Flush();
        }

        /// <summary>updates <see cref="LogTime"/> and creates a new file for the logs</summary>
        /// <returns>the <see cref="StreamWriter"/> for the new log file</returns>
        private static StreamWriter NewLogWriter()
        {
            string tFilePath, filePath, fileName;
            LogTime = DateTime.Now.Date;

            //get the file path
            fileName = LogTime.ToString("dd-MM-yyyy");
            tFilePath = Path.Combine(LogDirectory, $"{fileName}");
            filePath = tFilePath;

            //makes sure that the file or archive doesn't already exist
            for (int i = 1; File.Exists(filePath + ".log") || File.Exists(filePath + ".log.gz"); i++)
            {
                filePath = $"{tFilePath}_{i}";
            }

            //add extension
            filePath += ".log";

            //compress remaining log files
            CompressLogFiles();

            //create the file and instantiate the new StreamWriter onto the LogWriter
            FileStream fileStream = File.Create(filePath);
            StreamWriter writer = new(fileStream, Encoding.Unicode);

            //if LogWriter is not null, log that the file has changed
            if (LogWriter != null)
                Write($"Switched files!", "Logger", Severity.Warning);

            //return writer
            return writer;
        }

        /// <summary>
        /// compresses all the log files within the folder and deletes the original.
        /// </summary>
        private static void CompressLogFiles()
        {
            foreach (string path in Directory.GetFiles(LogDirectory))
            {
                if (Path.GetExtension(path) != ".log")
                    continue;

                //get the gZip path
                string gZipPath = Path.Combine(LogDirectory, Path.GetFileName(path) + ".gz");

                //get file streams
                var readStream = File.OpenRead(path); //read from path
                var createStream = File.Create(gZipPath); //create gZip file
                GZipStream gZipStream = new(createStream, CompressionLevel.SmallestSize); //create gZip file stream

                //copy contents
                readStream.CopyTo(gZipStream);

                //dispose the output stream
                gZipStream.DisposeAsync();

                //close read stream
                readStream.Close();

                //delete original
                File.Delete(path);
            }
        }

        /// <summary>
        /// handler for log messages
        /// </summary>
        /// <param name="_message">the log message</param>
        public delegate void LogHandler(LogMessage _message);

        /// <summary>
        /// event for when a new log message has been added
        /// </summary>
        public static event LogHandler NewLog;

        /// <summary>
        /// invokes <see cref="LogHandler"/> with the message
        /// </summary>
        /// <param name="_message">the log message</param>
        public static void Write(LogMessage _message)
            => NewLog.Invoke(_message);

        /// <inheritdoc cref="Write(LogMessage)"/>
        /// <remarks>
        /// initializes a <see cref="LogMessage"/> object with the <see cref="Severity.Info"/> severity
        /// </remarks>
        public static void Write(string _message, string _source)
            => NewLog.Invoke(new LogMessage(_message, _source, Severity.Info));

        /// <inheritdoc cref="Write(LogMessage)"/>
        /// <remarks>
        /// initializes a <see cref="LogMessage"/> object
        /// </remarks>
        public static void Write(string _message, string _source, Severity _severity)
            => NewLog.Invoke(new LogMessage(_message, _source, _severity));
    }
}