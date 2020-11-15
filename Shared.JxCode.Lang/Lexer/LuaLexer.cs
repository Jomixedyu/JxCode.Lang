using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.JxLexer
{
    //TODO
    public class LuaLexer<TKeyword> : LexerBase<TKeyword> where TKeyword : Enum
    {
        protected override IDictionary<char, LexerTokenType> _operations { get; set; } = new Dictionary<char, LexerTokenType>()
        {
            {'+' , LexerTokenType.OPR_PLUS },
            {'-' , LexerTokenType.OPR_MINUS },
            {'*' , LexerTokenType.OPR_MULTI },
            {'/' , LexerTokenType.OPR_DIVISION },
            {'=' , LexerTokenType.OPR_EQUAL },
            {'>' , LexerTokenType.OPR_GREATER },
            {'<' , LexerTokenType.OPR_LESS },
            {':' , LexerTokenType.OPR_COLON },
            {';' , LexerTokenType.OPR_SIMICOLON },
            {',' , LexerTokenType.OPR_COMMA },
            {'.' , LexerTokenType.OPR_DOT },
            {'(' , LexerTokenType.OPR_LPARENTHESIS },
            {')' , LexerTokenType.OPR_RPARENTHESIS },
            {'[' , LexerTokenType.OPR_LBRACKET },
            {']' , LexerTokenType.OPR_RBRACKET},
            {'{' , LexerTokenType.OPR_LBRACE },
            {'}' , LexerTokenType.OPR_RBRACE },
            {'?' , LexerTokenType.OPR_QUESTION } ,
            {'!' , LexerTokenType.OPR_EXCLAMATION } ,
            {'@' , LexerTokenType.OPR_AT },
            {'$' , LexerTokenType.OPR_DOLLAR },
            {'%' , LexerTokenType.OPR_PERCENT },
            {'^' , LexerTokenType.OPR_POWER },
            {'&' , LexerTokenType.OPR_AND },
            {'|' , LexerTokenType.OPR_OR },
            {'~' , LexerTokenType.OPR_TILDE },
            {'`' , LexerTokenType.OPR_GRAVEACCENT },
            {'#' , LexerTokenType.OPR_POUNDKEY },
            {'\n', LexerTokenType.LF }
        };
        protected override IDictionary<string, LexerTokenType> _doubleOperations { get; set; } = new Dictionary<string, LexerTokenType>()
        {
            {"++" , LexerTokenType.OPR_DPLUS },
            {"--" , LexerTokenType.OPR_DPLUS },
            {"==" , LexerTokenType.OPR_DEQUAL },
            {"~=" , LexerTokenType.OPR_NOTEQUAL }
        };

        protected override void getNextToken()
        {
            skipSpace();
            char nextC = peek();

            if (nextC == EOF)
            {
                //到结尾了
                getChar();
            }
            else if (nextC == LF)
            {
                getLF();
            }
            else if (nextC == '-')
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
        protected override void SkipNote()
        {
            getChar();
            char nextChar = peek();
            if (nextChar == '-')
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
    }
}
