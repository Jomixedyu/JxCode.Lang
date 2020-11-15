using System;
using System.Collections.Generic;
using System.Text;

namespace JxCode.Lang.JxLexer
{
    public enum LexerTokenType
    {
        UNKNOW, LF, EOF, IDENTIFIER, KEYWORD, NUM, STRING,

        OPR_DOT,   // .
        OPR_LBRACKET ,  // [
        OPR_RBRACKET ,  // ]
        OPR_LPARENTHESIS, // (
        OPR_RPARENTHESIS, // )

        OPR_MULTI, // *
        OPR_DIVISION, // /
        OPR_PERCENT, // %
        OPR_DPLUS,  // ++
        OPR_DMINUS, // --
        /// <summary>
        /// ~
        /// </summary>
        OPR_TILDE,
        /// <summary>
        /// 感叹号
        /// </summary>
        OPR_EXCLAMATION,

        OPR_PLUS,  // +
        OPR_MINUS, // -

        OPR_GREATER, // >
        OPR_LESS,   // <

        OPR_POWER, // ^
        OPR_AND,   // &
        OPR_OR,    // |
        OPR_DEQUAL, // ==
        OPR_NOTEQUAL, // != <> ~=
        OPR_EQUAL,  // =

        OPR_PLUS_EQ,
        OPR_MINUS_EQ,
        OPR_MUL_EQ,
        OPR_DIV_EQ,
        OPR_PERSENT_EQ,

        /// <summary>
        /// 逗号
        /// </summary>
        OPR_COMMA,

        /// <summary>
        /// 冒号
        /// </summary>
        OPR_COLON,
        /// <summary>
        /// 分号
        /// </summary>
        OPR_SIMICOLON,

        OPR_LBRACE,   // {
        OPR_RBRACE,   // }
        OPR_QUESTION,  // ?

        OPR_AT,
        OPR_DOLLAR,

        /// <summary>
        /// `
        /// </summary>
        OPR_GRAVEACCENT,
        /// <summary>
        /// #
        /// </summary>
        OPR_POUNDKEY,

    }
}
