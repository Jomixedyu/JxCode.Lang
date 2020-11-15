using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.JxLexer
{
    public class CLexer<TKeyword> : LexerBase<TKeyword> where TKeyword : Enum
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
            {"!=" , LexerTokenType.OPR_NOTEQUAL },

            {"+=" , LexerTokenType.OPR_PLUS_EQ },
            {"-=" , LexerTokenType.OPR_MINUS_EQ },
            {"*=" , LexerTokenType.OPR_MUL_EQ },
            {"/=" , LexerTokenType.OPR_DIV_EQ },
            {"%=" , LexerTokenType.OPR_PERSENT_EQ },
        };

    }
}
