using JxCode.Lang.JxLexer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace JxCode.Lang.NepInterpreter
{
    public enum ValueType
    {
        NULL, NUMBER, STRING, BOOLEAN, OBJECT
    }
    public class Value
    {
        private ValueType valueType;
        private object @object;

        public ValueType ValueType { get => valueType; }

        public float Number
        {
            get { return @object != null ? (float)@object : 0; }
            set { @object = value; valueType = ValueType.NUMBER; }
        }

        public bool Boolean
        {
            get { return @object != null ? (bool)@object : false; }
            set { @object = value; valueType = ValueType.BOOLEAN; }
        }

        public string String
        {
            get { return @object != null ? (string)@object : null; }
            set { @object = value; valueType = ValueType.STRING; }
        }

        public object Object
        {
            get => @object;
            set { @object = value; valueType = ValueType.OBJECT; }
        }

        public object CommonObject
        {
            get
            {
                if (this.valueType == ValueType.NULL)
                {
                    return null;
                }
                return this.@object;
            }
        }

        public Value() { }
        public Value(float number) : this() { this.valueType = ValueType.NUMBER; this.Number = number; }
        public Value(string @string) : this() { this.valueType = ValueType.STRING; this.String = @string; }
        public Value(bool @bool) : this() { this.valueType = ValueType.BOOLEAN; this.Boolean = @bool; }
        public Value(object @object) : this() { this.valueType = ValueType.OBJECT; this.Object = @object; }

        public void SetNull()
        {
            this.valueType = ValueType.NULL;
            this.@object = null;
        }
        public void NewValue(object value)
        {
            this.@object = Convert.ChangeType(value, this.@object.GetType());
        }

        public void CNumber()
        {
            switch (this.ValueType)
            {
                case ValueType.STRING:
                    this.Number = float.Parse(this.String);
                    break;
                case ValueType.NULL:
                case ValueType.BOOLEAN:
                    this.Number = this.Boolean == true ? 1 : 0;
                    break;
                case ValueType.OBJECT:
                    throw new ValueException("无法转换类型");
            }
            this.valueType = ValueType.NUMBER;
        }
        public void CString()
        {
            if (this.ValueType == ValueType.OBJECT && this.Object == null)
                throw new ValueNullPointerException("空指针");

            switch (this.ValueType)
            {
                case ValueType.NULL:
                    throw new ValueException("无法转换类型");
                case ValueType.NUMBER:
                    this.String = this.Number.ToString();
                    break;
                case ValueType.BOOLEAN:
                    this.String = this.Boolean.ToString();
                    break;
                case ValueType.OBJECT:
                    this.String = this.Object.ToString();
                    break;
            }
            this.valueType = ValueType.STRING;
        }
        public void CBoolean()
        {
            switch (ValueType)
            {
                case ValueType.STRING:
                    string str = this.String.Trim().ToLower();
                    if (str.Equals("true"))
                        this.Boolean = true;
                    else if (str.Equals("false"))
                        this.Boolean = false;
                    else
                        throw new ValueException("无法转换类型");
                    break;
                case ValueType.NULL:
                case ValueType.NUMBER:
                    this.Boolean = this.Number == 0 ? false : true;
                    break;
                case ValueType.OBJECT:
                    throw new ValueException("无法转换类型");
            }
            this.valueType = ValueType.BOOLEAN;
        }

        public Value BinaryOpr(Value b, LexerTokenType tokenType)
        {
            var a = this;

            if (a.ValueType == ValueType.NUMBER && b.ValueType == ValueType.NUMBER)
            {
                //数字连接
                switch (tokenType)
                {
                    case LexerTokenType.OPR_PLUS: return new Value(a.Number + b.Number);
                    case LexerTokenType.OPR_MINUS: return new Value(a.Number - b.Number);
                    case LexerTokenType.OPR_MULTI: return new Value(a.Number * b.Number);
                    case LexerTokenType.OPR_DIVISION: return new Value(a.Number / b.Number);
                    case LexerTokenType.OPR_DEQUAL: return new Value(a.Number == b.Number);
                    case LexerTokenType.OPR_NOTEQUAL: return new Value(a.Number != b.Number);
                    case LexerTokenType.OPR_GREATER: return new Value(a.Number > b.Number);
                    case LexerTokenType.OPR_LESS: return new Value(a.Number < b.Number);
                    default:
                        throw new ValueException("不支持的运算符");
                }
            }
            else
            {
                //字符串
                a.CString();
                b.CString();
                return new Value(a.String + b.String);
            }
        }

        public override string ToString()
        {
            switch (this.ValueType)
            {
                case ValueType.NULL: throw new ValueNullPointerException("空指针");
                case ValueType.NUMBER: return this.Number.ToString();
                case ValueType.STRING: return this.String;
                case ValueType.BOOLEAN: return this.Boolean.ToString();
                case ValueType.OBJECT:
                    if (this.Object == null)
                        throw new ValueNullPointerException("空指针");
                    else
                        return this.Object.ToString();
                default:
                    return string.Empty;
            }
        }
        //看object是否实现了Serialize和Deserialize方法。

        //变量序列化内容分隔符
        public string Serialize()
        {
            //格式： "ValueType;SerializeInfomation"
            string data = string.Empty;
            switch (valueType)
            {
                case ValueType.NULL:
                    data = "null";
                    break;
                case ValueType.NUMBER:
                case ValueType.BOOLEAN:
                case ValueType.STRING:
                    data = this.@object.ToString();
                    break;
                case ValueType.OBJECT:
                    var method = data.GetType().GetMethod("Serialize");
                    if (method != null)
                    {
                        //格式： "System.IO.StreamWriter:ObjectSerializeInfomation"
                        data = data.GetType().FullName + ":" + (string)method.Invoke(data, null);
                    }
                    break;
            }
            return this.ValueType.ToString() + ";" + data;
        }
        public static Value Deserialize(string state)
        {
            int op = state.IndexOf(";");
            string vTypeStr = state.Substring(0, op);
            string data = state.Substring(op);

            ValueType valueType = (ValueType)Enum.Parse(typeof(ValueType), vTypeStr);

            Value v = new Value();

            switch (valueType)
            {
                case ValueType.NULL:
                    v.SetNull();
                    break;
                case ValueType.NUMBER:
                    v.Number = float.Parse(data);
                    break;
                case ValueType.STRING:
                    v.String = data;
                    break;
                case ValueType.BOOLEAN:
                    v.Boolean = bool.Parse(data);
                    break;
                case ValueType.OBJECT:
                    int colPos = data.IndexOf(':');
                    string typePath = data.Substring(0, colPos);
                    string serInfo = data.Substring(colPos);

                    Type type = Type.GetType(typePath);
                    MethodInfo method = type.GetMethod("Deserialize");
                    if (method != null)
                    {
                        v.Object = method.Invoke(null, new object[] { serInfo });
                    }

                    break;
            }
            return v;
        }
    }

    public class ValueNullPointerException : ApplicationException
    {
        public ValueNullPointerException(string exceptionInfo) : base(exceptionInfo) { }
    }
    public class ValueException : ApplicationException
    {
        public ValueException(string exceptionInfo) : base(exceptionInfo) { }
    }
}