using JxCode.Lang.JxLexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using NepToken = JxCode.Lang.JxLexer.Token<JxCode.Lang.NepInterpreter.KeywordTokenType>;

namespace JxCode.Lang.NepInterpreter
{
    public class Module
    {
        public string MoudleName { get; private set; }

        public static readonly Dictionary<string, KeywordTokenType> keywords = new Dictionary<string, KeywordTokenType>()
        {
            //局部变量会在模块结束时释放，或者用delete来手动释放。
            {"set", KeywordTokenType.SET },                 {"设置", KeywordTokenType.SET},
            {"del" , KeywordTokenType.DELETE },             {"回收" , KeywordTokenType.DELETE},
            {"func" , KeywordTokenType.FUNCTION },          {"函数" , KeywordTokenType.FUNCTION },
            {"endfunc" , KeywordTokenType.ENDFUNCTION },    {"函数尾", KeywordTokenType.ENDFUNCTION },
            {"if" , KeywordTokenType.IF },                  {"如果" , KeywordTokenType.IF},
            {"then" , KeywordTokenType.THEN },              {"则" , KeywordTokenType.THEN},
            {"else" , KeywordTokenType.ELSE },              {"否则" , KeywordTokenType.ELSE},
            {"endif" , KeywordTokenType.ENDIF },            {"如果尾", KeywordTokenType.ENDIF},
            {"goto" , KeywordTokenType.GOTO },              {"跳转" , KeywordTokenType.GOTO},
            {"not" , KeywordTokenType.NOT },                {"取反" , KeywordTokenType.NOT},
            {"loop" , KeywordTokenType.LOOP },              {"循环", KeywordTokenType.LOOP},
            {"endloop" , KeywordTokenType.ENDLOOP },        {"循环尾" , KeywordTokenType.ENDLOOP},
            {"break" , KeywordTokenType.BREAK },            {"打断" , KeywordTokenType.BREAK},
            {"true" , KeywordTokenType.TRUE } ,             {"真" , KeywordTokenType.TRUE},
            {"false" , KeywordTokenType.FALSE },            {"假" , KeywordTokenType.FALSE}
        };

        private Dictionary<string, int> labels = new Dictionary<string, int>();

        private Interpreter parent;
        private IDictionary<string, Type> typeList;


        private List<List<NepToken>> lines;
        private int linePosition = -1;
        private List<NepToken> curLine;

        private int tokenPosition = -1;
        private NepToken curToken;

        private bool isNext = true;

        /// <summary>
        /// 重置上下文
        /// </summary>
        public void ResetContext()
        {
            this.linePosition = -1;
            this.curLine = null;
            this.tokenPosition = -1;
            this.curToken = null;
        }
        /// <summary>
        /// 跳转至标签
        /// </summary>
        /// <param name="labelName"></param>
        public void Goto(string labelName)
        {
            if (!this.labels.ContainsKey(labelName))
            {
                throw new ModuleException(null, labelName + "跳转标签不存在");
            }
            int pos = this.labels[labelName];
            this.linePosition = pos;
            this.curLine = this.lines[pos];
            this.tokenPosition = -1;
            this.curToken = null;
        }

        public Module(
            Interpreter parent,
            string moduleName,
            IDictionary<string, Type> typeList,
            string code)
        {
            this.parent = parent;
            this.MoudleName = moduleName;
            this.typeList = typeList;

            var lex = new NepLexer<KeywordTokenType>();
            var tokenStream = lex.Scanner(code, keywords);

            this.lines = new List<List<NepToken>>();
            int i = 0;
            this.lines.Add(new List<NepToken>());
            foreach (var item in tokenStream)
            {
                if (item.TokenType == LexerTokenType.LF)
                {
                    i++;
                    this.lines.Add(new List<NepToken>());
                    continue;
                }
                this.lines[i].Add(item);
            }
            //分析所有行，筛选出label的位置
            for (int lineIndex = 0; lineIndex < this.lines.Count; lineIndex++)
            {
                List<NepToken> item = this.lines[lineIndex];
                if (item.Count == 5)
                {
                    if (item[0].TokenType == LexerTokenType.OPR_COLON &&
                        item[1].TokenType == LexerTokenType.OPR_COLON &&
                        item[2].TokenType == LexerTokenType.IDENTIFIER &&
                        item[3].TokenType == LexerTokenType.OPR_COLON &&
                        item[4].TokenType == LexerTokenType.OPR_COLON)
                    {
                        this.labels.Add(item[2].Value, lineIndex);
                    }
                }
            }
        }

        /// <summary>
        /// 继续运行
        /// </summary>
        public void Next()
        {
            isNext = true;
            while (isNext)
            {
                Execute();
            }
        }
        /// <summary>
        /// 暂停运行
        /// </summary>
        public void End()
        {
            isNext = false;
        }
        /// <summary>
        /// 获取一行token并向前移动
        /// </summary>
        /// <returns></returns>
        private List<NepToken> getLine()
        {
            this.tokenPosition = -1;
            this.curToken = null;

            this.linePosition++;

            if (this.lines.Count <= this.linePosition)
                return null;
            else
                this.curLine = this.lines[this.linePosition];
            return this.curLine;
        }
        /// <summary>
        /// 获取一个token并向前移动
        /// </summary>
        /// <returns></returns>
        private NepToken getToken()
        {
            if (curLine == null) return null;
            this.tokenPosition++;

            if (this.curLine.Count <= this.tokenPosition)
                return null;
            else
                this.curToken = this.curLine[this.tokenPosition];
            return this.curToken;
        }
        /// <summary>
        /// 偷看下一个token
        /// </summary>
        /// <returns></returns>
        private NepToken peekToken()
        {
            if (curLine == null) return null;
            if (this.curLine.Count - 1 <= this.tokenPosition)
                return null;
            else
                return this.curLine[this.tokenPosition + 1];
        }
        /// <summary>
        /// 执行一行语句
        /// </summary>
        private void Execute()
        {
            var line = getLine();

            if (line == null)
            {
                End();
                return;
            }

            var token = peekToken();
            if (token is null)
            {

            }
            else
            {
                switch (token.TokenType)
                {
                    //关键字
                    case LexerTokenType.KEYWORD:
                        exeKeyword();
                        break;
                    case LexerTokenType.IDENTIFIER:
                        //方法调用
                        List<NepToken> tokens = getMemberPath();
                        var com = getToken();
                        if (com == null || com.TokenType != LexerTokenType.OPR_COLON)
                            throw new ModuleException(token, "语法错误");
                        if (this.typeList.ContainsKey(tokens[0].Value))
                        {
                            //静态方法
                            invokeMethod(tokens, null);

                        }
                        else if (this.parent.Variables.ContainsKey(tokens[0].Value))
                        {
                            //实例方法
                            invokeMethod(tokens, null, this.parent.Variables[tokens[0].Value].Object);
                        }
                        else
                        {
                            throw new ModuleException(curToken, "未知标识符");
                        }

                        break;
                }
            }
        }
        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="ret"></param>
        /// <param name="instance"></param>
        private void invokeMethod(List<NepToken> tokens, Value ret = default, object instance = null)
        {
            List<NepToken> valueInfos = new List<NepToken>();

            bool acceptValue = true;
            while (true)
            {
                var token = getToken();
                //TODO 表达式
                if (token is null) break;
                if (acceptValue)
                {
                    if (token.TokenType == LexerTokenType.OPR_COMMA)
                    {
                        valueInfos.Add(null);
                        acceptValue = true;
                    }
                    else
                    {
                        valueInfos.Add(token);
                        acceptValue = false;
                    }

                }
                else
                {
                    if (token.TokenType != LexerTokenType.OPR_COMMA)
                        throw new ModuleException(token, "语法错误");
                    acceptValue = true;
                }
            }


            MethodInfo mi = this.getRMethod(tokens, instance == null, valueInfos);
            if (mi == null)
            {
                NepToken token = tokens.Count == 0 ? null : tokens[0];
                throw new ModuleException(token, "没有找到匹配的方法");
            }

            ParameterInfo[] parmInfos = mi.GetParameters();


            //方法有两种方式，非标准方法是第一二形参是ModuleRuntimeInfo和Value类型的，另一种就是是标准方法
            //非标准方法中的Module用于脚本控制，而Value则是函数返回值
            object[] paramObj = new object[parmInfos.Length];
            bool isStdMethod = true;
            int valueOffset = 0;
            if (parmInfos.Length >= 2 &&
                parmInfos[0].ParameterType == typeof(NepInvokeEvent) &&
                parmInfos[1].ParameterType == typeof(Value))
            {
                paramObj[0] = new NepInvokeEvent(this.parent, this.MoudleName);
                paramObj[1] = ret;
                valueOffset = -2;
                isStdMethod = false;
            }

            int i = isStdMethod ? 0 : 2;

            for (; i < parmInfos.Length; i++)
            {
                var paramInfo = parmInfos[i];
                var valueInfo = valueInfos[i + valueOffset];
                if (valueInfo != null)
                {
                    //如果是标识符则取值
                    if (valueInfo.TokenType == LexerTokenType.IDENTIFIER)
                    {
                        if (this.parent.Variables.ContainsKey(valueInfo.Value))
                        {
                            Type paramType = paramInfo.ParameterType;
                            object value = this.parent.Variables[valueInfo.Value].CommonObject;
                            Type valueType = value.GetType();
                            //如果对象是形参的子类则直接传入
                            if (valueType.IsSubclassOf(paramType))
                            {
                                paramObj[i] = value;
                            }
                            else
                            {
                                //否则要转一下
                                paramObj[i] = Convert.ChangeType(value, paramType);
                            }
                        }
                        else
                        {
                            throw new ModuleException(valueInfo, "无法查找到此变量，错误的标识符");
                        }
                    }
                    else
                    {
                        //字面量
                        paramObj[i] = Convert.ChangeType(valueInfo.Value, paramInfo.ParameterType);
                    }
                }

            }
            //标准函数会正常返回值
            //而非标准函数只能返回布尔，true为继续执行，false为中断执行
            var result = mi.Invoke(instance, paramObj);
            if (result != null)
            {
                if (isStdMethod)
                {
                    ObjToValue(result, ret);
                }
                else
                {
                    if ((bool)result == true)
                        isNext = true;
                    else
                        isNext = false;
                }
            }
        }
        /// <summary>
        /// 执行关键字
        /// </summary>
        private void exeKeyword()
        {
            NepToken token = getToken();
            switch (token.Keyword)
            {
                case KeywordTokenType.SET:
                    List<NepToken> tokens = getMemberPath();
                    var eq = getToken(); //=

                    if (eq.TokenType != LexerTokenType.OPR_EQUAL)
                        throw new ModuleException(curToken, "赋值语句错误");
                    if (this.parent.Variables.ContainsKey(tokens[0].Value))
                    {
                        //变量
                        setVar(tokens[0].Value, getValue());
                    }
                    else if (this.typeList.ContainsKey(tokens[0].Value))
                    {
                        //静态字段
                        FieldInfo m = (FieldInfo)getRMember(tokens, true);
                        var v = getValue();
                        m.SetValue(null, Convert.ChangeType(v.CommonObject, m.FieldType));
                    }
                    else
                    {
                        //新变量
                        setVar(tokens[0].Value, getValue());
                    }
                    break;
                case KeywordTokenType.GOTO:
                    getToken();
                    this.Goto(this.curToken.Value);
                    break;
            }
        }
        /// <summary>
        /// 获取Token的Type
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private Type getTokenType(NepToken token)
        {
            switch (token.TokenType)
            {
                case LexerTokenType.NUM:
                    return typeof(float);
                case LexerTokenType.STRING:
                    return typeof(string);
                case LexerTokenType.KEYWORD:
                    if (token.Keyword == KeywordTokenType.TRUE ||
                        token.Keyword == KeywordTokenType.FALSE)
                    {
                        return typeof(bool);
                    }
                    else
                    {
                        return null;
                    }
                case LexerTokenType.IDENTIFIER:
                    if (this.parent.Variables.ContainsKey(token.Value))
                    {
                        return this.parent.Variables[token.Value].GetType();
                    }
                    else
                    {
                        return null;
                    }
            }
            return null;
        }
        /// <summary>
        /// 获取匹配的方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isStatic"></param>
        /// <param name="valueInfos"></param>
        /// <returns></returns>
        private MethodInfo getRMethod(List<NepToken> path, bool isStatic, List<NepToken> valueInfos)
        {
            //方法所在的类型
            Type type = isStatic ? this.typeList[path[0].Value] : this.parent.Variables[path[0].Value].Object.GetType();
            //查找匹配的方法
            Type[] invokeParamTypes = new Type[valueInfos.Count + 2];
            invokeParamTypes[0] = typeof(NepInvokeEvent);
            invokeParamTypes[1] = typeof(Value);

            Type[] paramTypes = new Type[valueInfos.Count];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                //如果token是Value，则是传入了个变量，需要寻找到变量的值
                Type vInfo = this.getTokenType(valueInfos[i]);
                if(vInfo == typeof(Value))
                {
                    Value _var = this.parent.Variables[valueInfos[i].Value];
                    paramTypes[i] = _var.CommonObject.GetType();
                }
                else
                {
                    paramTypes[i] = vInfo;
                }
                
                invokeParamTypes[i + 2] = paramTypes[i];
            }

            //从第2个（因为上面第一个已经获取到了）查询到倒数第二个（最后一个是方法名）
            for (int i = 1; i < path.Count - 1; i++)
            {
                var t = type.GetMember(path[i].Value);
                if (t.Length == 0)
                    throw new ModuleException(path[i], "成员不存在");
                type = t[0].DeclaringType;
            }
            string methodName = path[path.Count - 1].Value;

            MethodInfo method = null;

            //先用获取带有事件参数的方法
            method = type.GetMethod(methodName, invokeParamTypes);
            //如果没有在获取普通方法
            if (method == null)
            {
                method = type.GetMethod(methodName, paramTypes);
            }
            return method;
        }
        /// <summary>
        /// 获取成员反射信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private MemberInfo getRMember(List<NepToken> path, bool isStatic)
        {
            Type type = isStatic ? this.typeList[path[0].Value] : this.parent.Variables[path[0].Value].Object.GetType();
            for (int i = 1; i < path.Count - 1; i++)
            {
                var t = type.GetMember(path[i].Value);
                if (t.Length == 0)
                    throw new ModuleException(path[i], "成员不存在");
                type = t[0].DeclaringType;
            }
            var members = type.GetMember(path[path.Count - 1].Value);
            if (members.Length == 0)
            {
                throw new ModuleException(null, "成员不存在");
            }
            return members[0];
        }
        /// <summary>
        /// 向前移动，获取一个路径
        /// </summary>
        /// <returns></returns>
        private List<NepToken> getMemberPath()
        {
            List<NepToken> tokens = new List<NepToken>();
            while (peekToken() != null &&
                (peekToken().TokenType == LexerTokenType.IDENTIFIER ||
                peekToken().TokenType == LexerTokenType.OPR_DOT))
            {
                var token = getToken();
                if (token.TokenType == LexerTokenType.IDENTIFIER)
                    tokens.Add(token);
            }
            return tokens;
        }
        private Value ObjToValue(object obj, Value v = null)
        {
            if (v == null) v = new Value();
            Type type = obj.GetType();
            if (type == typeof(float))
                v.Number = (float)obj;
            else if (type == typeof(double))
                v.Number = (float)(double)obj;
            else if (type == typeof(int))
                v.Number = (float)(int)obj;
            else if (type == typeof(bool))
                v.Boolean = (bool)obj;
            else if (type == typeof(string))
                v.String = (string)obj;
            else
                v.Object = obj;
            return v;
        }
        /// <summary>
        /// 获取一个右值表达式结果
        /// </summary>
        /// <returns></returns>
        private Value getValue()
        {
            var token = peekToken();
            switch (token.TokenType)
            {
                case LexerTokenType.IDENTIFIER:
                    if (this.parent.Variables.ContainsKey(token.Value))
                    {
                        return this.parent.Variables[token.Value];
                        //基元类型
                        Value v = this.parent.Variables[token.Value];
                        v.NewValue(token.Value);

                        //动态类型
                    }
                    else
                    {
                        //静态
                        if (this.typeList.ContainsKey(token.Value))
                        {
                            //类型存在
                            Type type = this.typeList[token.Value];
                            var path = getMemberPath();
                            var peek = peekToken();
                            //从第二个元素开始，获取除最后一个元素
                            for (int i = 1; i < path.Count - 1; i++)
                            {
                                var t = type.GetMember(path[i].Value);
                                if (t.Length == 0)
                                    throw new ModuleException(path[i], "成员不存在");
                                type = t[0].DeclaringType;
                            }

                            if (peek == null || peek.TokenType != LexerTokenType.OPR_COLON)
                            {
                                //字段
                                getToken();
                                FieldInfo fi = type.GetField(path[path.Count - 1].Value);
                                var obj = fi.GetValue(null);
                                return ObjToValue(obj);
                            }
                            else
                            {
                                //方法
                                getToken();
                                Value value = new Value();
                                invokeMethod(path, value);
                                if (value.CommonObject == null)
                                    return null;
                                else
                                    return value;
                            }
                        }
                        else
                        {
                            throw new ModuleException(curToken, "未知类型");
                        }
                    }
                    return null;
                case LexerTokenType.NUM:
                    token = getToken();
                    return new Value(float.Parse(token.Value));
                case LexerTokenType.STRING:
                    token = getToken();
                    return new Value(token.Value);
                case LexerTokenType.KEYWORD:
                    token = getToken();
                    if (token.Keyword == KeywordTokenType.TRUE)
                        return new Value(true);
                    else if (token.Keyword == KeywordTokenType.FALSE)
                        return new Value(false);
                    else
                        throw new ModuleException(curToken, "无法将关键字赋值");
                    break;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 设置一个变量至虚拟机
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        private void setVar(string name, Value v)
        {
            if (this.parent.Variables.ContainsKey(name))
            {
                if (v is null)
                    this.parent.Variables.Remove(name);
                else
                    this.parent.Variables[name] = v;
            }
            else
            {
                if (v != null)
                    this.parent.Variables.Add(name, v);
            }
        }

        public ModuleRuntimeState GetSerializeObject()
        {
            ModuleRuntimeState mrs = new ModuleRuntimeState()
            {
                moduleName = this.MoudleName,
                linePosition = this.linePosition,
                tokenPosition = this.tokenPosition,
            };
            return mrs;
        }
        public static Module Deserialize(Interpreter interpreter, string code, ModuleRuntimeState state)
        {
            Module m = new Module(interpreter, state.moduleName, interpreter.GetTypes(), code);
            m.linePosition = state.linePosition;
            m.tokenPosition = state.tokenPosition;
            m.MoudleName = state.moduleName;
            return m;
        }
    }

    /// <summary>
    /// 模块运行时状态
    /// </summary>
    public class ModuleRuntimeState
    {
        public string moduleName;
        public int linePosition;
        public int tokenPosition;
    }

    public class ModuleException : ApplicationException
    {
        public ModuleException(Token<KeywordTokenType> token, string message)
            : base(message + " " + token?.ToString())
        {

        }
    }

    public enum KeywordTokenType
    {
        NONE, SET, DELETE,
        NULL, FUNCTION, ENDFUNCTION,
        IF, THEN, ELSE, ENDIF,
        GOTO, BEGIN, NOT,
        LOOP, ENDLOOP, BREAK, TRUE, FALSE,
    }
}
