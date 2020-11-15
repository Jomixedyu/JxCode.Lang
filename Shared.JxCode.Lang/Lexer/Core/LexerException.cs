using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.JxLexer
{
    public class LexerException : ApplicationException
    {
        private int line;
        private int charNum;
        public int Line { get => line; }
        public LexerException(string message, int line, int charNum)
        {
            this.line = line;
            this.message = message;
            this.charNum = charNum;
        }
        private string message;
        public override string Message
        {
            get
            {
                return message + " 行:" + line + "位置:" + charNum;
            }
        }
    }
}
