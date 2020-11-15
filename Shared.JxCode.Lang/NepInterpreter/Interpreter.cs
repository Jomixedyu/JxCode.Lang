using System;
using System.Collections.Generic;
using System.Text;
using JxCode.Lang.NepInterpreter.Lib;

namespace JxCode.Lang.NepInterpreter
{
    /*
     * 关于序列化与反序列化:
     * 解释器执行序列化时同时会对所有模块和变量进行序列化
     * 如果变量储存了一个对象，那么该对象的类型需要实现以下两个方法
     * public string Serialize();
     * public static object Deserialize(string str);
     */

    /*
     * 关于C#侧函数：
     * 脚本侧只可以调用C#侧函数，无法自行声明函数
     * C#侧的方法声明有两种
     *     1. 普通的方法，将参数一对一的传入，返回值同等于脚本侧返回值
     *     2. 流程控制的方法：
     *        需要方法签名： public static bool MethodName(NepInvokeEvent info, Value ret, ...)
     *        形参列表中的第一个和第二个是固定的
     *            ModuleRuntimeInfo info: 模块运行时的信息，可以获取调用此方法的模块和解释器等信息。
     *            ref Value ret: 函数返回值，该返回值会返回至脚本侧
     *        返回值bool可以省略，省略默认返回false
     *            返回true   解释器将继续向下执行
     *            返回false  解释器将暂停执行
     */

    /*
     * 关于脚本的变量声明储存与销毁
     * 脚本解释器内的所有模块全部使用全局变量
     * 变量使用set关键字设置
     * 使用 = null 或者 del关键字回收
     */
    public class Interpreter
    {
        //可反射调用的类型集
        private Dictionary<string, Type> typeList = new Dictionary<string, Type>();
        //所有模块
        private Dictionary<string, Module> modules = new Dictionary<string, Module>();
        //状态机
        private Module curModuleState;
        //触发器
        private List<string> triggers;
        //调用栈
        private Stack<string> callStack;

        //全局变量
        public Dictionary<string, Value> Variables { get; private set; } = new Dictionary<string, Value>();

        //加载代码函数
        private Func<string, string> loadCodeFunc;

        public Module this[string moduleName] { get => this.modules[moduleName]; }

        public Interpreter(Func<string, string> loadCode)
        {
            this.loadCodeFunc = loadCode;
            this.AddType(typeof(math));
        }

        public Interpreter AddType(Type type)
        {
            string typeName = type.Name;
            if (this.typeList.ContainsKey(typeName))
            {
                return this;
            }
            this.typeList.Add(typeName, type);
            return this;
        }
        public IDictionary<string, Type> GetTypes()
        {
            return this.typeList;
        }

        public Interpreter AddScript(string scriptName)
        {
            if (this.modules.ContainsKey(scriptName))
            {
                return this;
            }
            string code = this.loadCodeFunc(scriptName);
            Module m = new Module(this, scriptName, this.typeList, code);
            this.modules.Add(scriptName, m);
            return this;
        }
        public void RemoveScript(string scriptName)
        {
            if (!this.modules.ContainsKey(scriptName))
            {
                return;
            }
            this.modules.Remove(scriptName);
        }
        public void SetCurrent(string scriptName)
        {
            this.curModuleState = this.modules[scriptName];
        }

        [Obsolete("TODO", true)]
        public void CallModule(string scriptName, string labelName)
        {
            //调用栈的返回值
            this.callStack.Push(this.curModuleState.MoudleName);
            this.SetCurrent(scriptName);
            this.curModuleState.Goto(labelName);
        }

        public void AddNextTriggerLabel(string name)
        {
            this.triggers.Add(name);
        }
        public void RemoveNextTriggerLabel(string name)
        {
            if (this.triggers.Contains(name))
            {
                this.triggers.Remove(name);
            }
        }
        public void NextTrigger(string name)
        {
            if (this.triggers.Contains(name))
            {
                this.triggers.Remove(name);
                this.curModuleState.Next();
            }
        }
        public void Next()
        {
            this.curModuleState.Next();
        }

    }
    public class NepInvokeEvent
    {
        private Interpreter interpreter;
        private string moduleName;
        public Interpreter sender { get => this.interpreter; }
        public string ModuleName { get => this.moduleName; }

        public NepInvokeEvent(Interpreter interpreter, string moduleName)
        {
            this.interpreter = interpreter;
            this.moduleName = moduleName;
        }
    }

}
