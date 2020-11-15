using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.JxLexer
{
    public class Token<TEnum> where TEnum : Enum
    {
        public string Value { get; set; }
        public LexerTokenType TokenType { get; set; }
        public TEnum Keyword { get; set; }
        public int LineNum { get; set; }
        public int Position { get; set; }
        public Token()
        {

        }
        public Token(string value, LexerTokenType tokenType, int lineNum, int position)
        {
            Value = value;
            TokenType = tokenType;
            LineNum = lineNum;
            Position = position;
        }
        public Token(string value, LexerTokenType tokenType, int lineNum, int position, TEnum keyword)
            : this(value, tokenType, lineNum, position)
        {
            Keyword = keyword;
        }

        public override string ToString()
        {
            string value1 = string.Empty;
            if (TokenType == LexerTokenType.KEYWORD)
                value1 = Keyword.ToString();
            else
                value1 = Value;
            StringBuilder sb = new StringBuilder(128);
            sb.Append("Token value:");
            sb.Append(Value);
            sb.Append(", type:");
            sb.Append(TokenType.ToString());
            if (TokenType == LexerTokenType.KEYWORD)
                sb.Append(", key:" + Keyword.ToString());
            sb.Append(", lineNum:");
            sb.Append(LineNum.ToString());
            sb.Append(", position:");
            sb.Append(Position.ToString());
            return sb.ToString();
        }
        public string GetInfo()
        {
            return string.Format("行:{0} 列:{1} 关键:{2}", LineNum, Position, Value);
        }
        public bool IsValue()
        {
            return TokenType == LexerTokenType.NUM ||
                   TokenType == LexerTokenType.STRING;
        }
    }
}
