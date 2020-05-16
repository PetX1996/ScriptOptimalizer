using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class Function
    {
        public string Name { get; set; }
        public string Signature { get; set; }
        public string Content { get; set; }

        public Function( string name, string signature, string content )
        {
            this.Name = name;
            this.Signature = signature;
            this.Content = content;
        }

        public override string ToString()
        {
            return "Function \'"+this.Name+this.Signature+"\'\n"+this.Content;
        }
    }

    class FunctionList : List<Function>
    {
        public void Add( string name, string signature, string content )
        { 
            if(this.FindIndex(a => a.Name == name) != -1)
            {
                Debug.Error( "Function \'"+name+"\' already defined! Line: "+Program.DEBUG_LineI );
                return;
            }

            this.Add(new Function(name, signature, content));
        }

        public override string ToString()
        {
            Debug.Separator();
            Debug.WriteLine("Function List");
            Debug.Separator();
            foreach(Function f in this)
            {
                Debug.Write(f.ToString()+"\n");
                Debug.Separator();
            }

            return "";
        }
    }
}
