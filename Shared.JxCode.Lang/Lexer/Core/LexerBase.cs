using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JxCode.Lang.JxLexer
{
    public abstract class LexerBase<TKeyword> where TKeyword : Enum
    {
        protected const char EOF = '\0';
        protected const char LF = '\n';
        protected const char CR = '\r';
        protected const char SPACE = ' ';

        protected IDictionary<string, TKeyword> _keywords;
        protected abstract IDictionary<char, LexerTokenType> _operations { get; set; }

        protected abstract IDictionary<string, LexerTokenType> _doubleOperations { get; set; }

        //Tokens
        protected List<Token<TKeyword>> Tokens;
        //当前读取到了行数
        protected int lineNum = 1;
        //当前字符在所在行的位置
        protected int charPosition = 0;
        //当前单词的长度
        protected int wordLength = 0;
        //当前的字符
        protected char curChar;
        //源代码
        protected string sourceCode;
        protected int sourceCodePosition;
        //重置
        protected void Reset()
        {
            lineNum = 1;
            charPosition = 0;
            wordLength = 0;
            curChar = EOF;
            sourceCode = string.Empty;
            sourceCodePosition = -1;
        }

        /// <summary>
        /// 开始扫描
        /// </summary>
        /// <param name="code">源代码</param>
        /// <param name="keywords">关键字字典</param>
        /// <returns></returns>
        public List<Token<TKeyword>> Scanner(string code, IDictionary<string, TKeyword> keywords)
        {
            //初始化
            Reset();
            Tokens = new List<Token<TKeyword>>();
            sourceCode = code;
            _keywords = keywords;


            getNextToken();
            //运行到文件结尾
            while (curChar != EOF)
            {
                getNextToken();
            }

            return Tokens;
        }

        //获取下一个token
        protected virtual void getNextToken()
        {
            skipSpace();
            char nextC = peek();

            if (nextC == EOF)
            {
                //到结尾了
                getChar();
            }
            else if(nextC == LF)
            {
                getLF();
            }
            else if (nextC == '/')
            {
                //是一个备注
                SkipNote();
            }
            else if (nextC == '\"')
            {
                //是字符串
                getString();
            }
            else if (isWord(nextC))
            {
                //可以是关键字或标识符
                getIdentifier();
            }
            else if (isNum(nextC))
            {
                //数字
                getNumber();
            }
            else if (isOperator(nextC))
            {
                //符号
                getOperation();
            }
        }
        protected void getLF()
        {
            getChar();
            charPosition = 0;
            wordLength = 0;
            lineNum++;
            AddToken("\n", LexerTokenType.LF);
        }
        //获取一个数字
        protected virtual void getNumber()
        {
            StringBuilder sb = new StringBuilder(32);
            getChar();
            sb.Append(curChar);
            bool hasDot = false;
            while (char.IsNumber(peek()) || peek() == '.')
            {
                getChar();
                //数字中出现了第二个小数点
                if (hasDot && curChar == '.')
                {
                    throw new LexerException("错误：数字不能有两个小数点", lineNum, charPosition);
                }
                if (curChar == '.')
                    hasDot = true;
                sb.Append(curChar);
            }
            AddToken(sb.ToString(), LexerTokenType.NUM);
            sb.Clear();
        }

        /// <summary>
        /// 获取一个关键字或标识符，关键字和标识符最长64个字符！
        /// </summary>
        protected virtual void getIdentifier()
        {
            char[] chars = new char[64];
            int p = 0;

            while (!_operations.ContainsKey(peek()))
            {
                getChar();
                //遇到不是字符下划线和数字就返回
                if (!isIdent(curChar)) break;
                chars[p] = curChar;
                p++;
            }
            //裁剪添加
            string ret = new string(chars, 0, p);
            //是标识符或者关键字
            if (_keywords.ContainsKey(ret))
            {
                //是关键字
                AddToken(ret, _keywords[ret]);
            }
            else
            {
                //是标识符
                AddToken(ret, LexerTokenType.IDENTIFIER);
            }
        }
        /// <summary>
        /// 获取一个符号
        /// </summary>
        protected virtual void getOperation()
        {
            char[] opr = new char[2];
            opr[0] = getChar();
            //可能是双符号
            bool binaryOperationable = false;
            switch (curChar)
            {
                case '+':
                case '-':
                case '=':
                case '<':
                case '>':
                    binaryOperationable = true;
                    break;
                default:
                    break;
            }
            if (binaryOperationable)
            {
                string bo = curChar.ToString() + peek();
                if (_doubleOperations.ContainsKey(bo))
                {
                    //确实是双符号
                    opr[1] = getChar();
                    string str = new string(opr);
                    AddToken(str, _doubleOperations[str]);
                    return;
                }
            }
            AddToken(opr[0].ToString(), _operations[opr[0]]);
        }
        /// <summary>
        /// 获取一个字符串
        /// </summary>
        protected virtual void getString()
        {
            StringBuilder sb = new StringBuilder(128);
            next();
            while (true)
            {
                getChar();
                //如果不为双引号则添加进去
                if (curChar != '\"')
                    sb.Append(curChar);
                //遇到转义字符，直接把下一个也加进去然后重新循环
                if (curChar == '\\')
                {
                    getChar();
                    sb.Append(curChar);
                    continue;
                }
                if (curChar == '\"')
                {
                    break;
                }
            }
            //替换转义符为正确的字符
            AddToken(sb.ToString()
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                , LexerTokenType.STRING);
        }

        /// <summary>
        /// 跳过备注
        /// </summary>
        protected virtual void SkipNote()
        {
            getChar();
            char nextChar = peek();
            if (nextChar == '/')
            {
                //单行注释，查询到结尾
                next(); //前进一个字符
                while (true) //直到前进到换行为止
                {
                    getChar();
                    if (curChar == LF)
                        break;
                    if (curChar == EOF)
                        break;
                }

            }
            else if (nextChar == '*')
            {
                //多行注释
                next();
                getChar();
                while (true)//直到结尾
                {
                    if (curChar == '*' && peek() == '/')
                    {
                        next();
                        break;
                    }
                    if (curChar == EOF)
                    {
                        //到结尾了没闭合
                        throw new LexerException("注释块错误", lineNum, charPosition);
                    }
                    getChar();
                }
            }
            else
            {
                //不到啥玩意
                throw new LexerException("注释块错误", lineNum, charPosition);
            }
        }
        /// <summary>
        /// 开头是否为数字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool isNum(char c)
        {
            return char.IsNumber(c);
        }
        /// <summary>
        /// 开头是否可以是可用字 /标识符/关键字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool isWord(char c)
        {
            return char.IsLetter(c) || c == '_';
        }
        /// <summary>
        /// 是否可以是可用字 /标识符/关键字，比isword多了数字，因为标识符和关键字不能以数字开头
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool isIdent(char c)
        {
            return isWord(c) || isNum(c);
        }
        /// <summary>
        /// 开头是否为符号
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool isOperator(char c)
        {
            return _operations.ContainsKey(c);
        }
        /// <summary>
        /// 清除连续空格
        /// </summary>
        protected void skipSpace()
        {
            while (NextIsSpaceOrEnter())
            {
                next();
            }
        }
        /// <summary>
        /// 获取下一字符
        /// </summary>
        /// <returns></returns>
        protected char peek(int offset = 1)
        {
            if (this.sourceCodePosition + offset >= this.sourceCode.Length)
                return EOF;
            else
                return this.sourceCode[this.sourceCodePosition + offset];
        }
        /// <summary>
        /// 获取下一字符并前进
        /// </summary>
        /// <returns></returns>
        protected char getChar()
        {
            this.sourceCodePosition++;

            if (this.sourceCodePosition >= this.sourceCode.Length)
                this.curChar = EOF;
            else
                this.curChar = this.sourceCode[this.sourceCodePosition];

            //增加字符位置数
            this.charPosition++;
            this.wordLength++;

            return this.curChar;
        }
        /// <summary>
        /// 向下一字符前进
        /// </summary>
        protected void next()
        {
            getChar();
        }
        /// <summary>
        /// 下一个字符是否为空格
        /// </summary>
        /// <returns></returns>
        protected bool NextIsSpaceOrEnter()
        {
            char c = peek();
            if (c == ' ' || c == '\t' || c == '\r')
                return true;
            else
                return false;
        }
        /// <summary>
        /// 添加到Token列表中
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        protected void AddToken(string str, LexerTokenType type)
        {
            Tokens.Add(new Token<TKeyword>(str, type, lineNum, charPosition - wordLength));
            wordLength = 0;
        }
        protected void AddToken(string str, TKeyword type)
        {
            Tokens.Add(new Token<TKeyword>(str, LexerTokenType.KEYWORD, lineNum, charPosition - wordLength, type));
            wordLength = 0;
        }
       
    }

}
