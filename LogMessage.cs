using System.ComponentModel.DataAnnotations;

namespace Logger
{
    public class LogMessage
    {
        /// <summary>the formatted log message</summary>
        public string Message { get; }

        /// <summary>the unformatted log message</summary>
        public string MessageRaw { get; }

        /// <summary>the source of the log message</summary>
        public string Source { get; }

        /// <summary>the severity of the log message</summary>
        public Severity Severity { get; }

        /// <summary>the time of the log message</summary>
        public DateTime Time { get; }

        /// <summary>
        /// constructs a log message
        /// </summary>
        /// <param name="_message">the message of the log message, </param>
        /// <param name="_source">the source of the message, max length of 10</param>
        /// <param name="_severity">the severity of the message</param>
        public LogMessage(string _message, string _source, Severity _severity = Severity.Info)
        {
            //replaces illegal characters in the message
            _message = _message.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\b", "\\b").Replace("\r", "\\r").Replace("\0", "\\0");

            //checks if the source is not too long, resizes it if it is
            if (_source.Length > 10)
                _source = _source[0..9];

            string time, severity, source, w;

            //set properties
            MessageRaw = _message;
            Source = _source;
            Time = DateTime.Now;
            Severity = _severity;

            //format the time
            time = Time.ToString("HH:mm:ss");

            severity = _severity.ToString();
            w = new string(' ', 7 - severity.Length);

            //format source with whitespace
            source = _source;
            source += new string(' ', 11 - source.Length);

            //add everything together to the message property
            Message = $"[{time}] [{severity}]{w} {source} {_message}";
        }
    }
}
