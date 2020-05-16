using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class Replace
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Replace(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            return "Replace \'" + this.Name + "\' = \'" + this.Value + "\'";
        }
    }

    class ReplaceList : List<Replace>
    {
        public void Add(string name) { Add(name, ""); }
        public void Add(string name, string value)
        {
            int index = this.FindIndex(a => a.Name == name);
            if (index != -1)
            {
                Debug.Warning("Redefinition of replace \'" + name + "\' Line: "+Program.DEBUG_LineI);
                this.RemoveAt(index);
            }

            this.Add(new Replace(name, value));
        }

        /*public void AddRange(ReplaceList list, bool ignorePerm)
        {
            foreach (Replace r in list)
            {
                if(ignorePerm && !r.Permanent)
                    this.Add(r.Name, r.Value, r.Permanent);
                else if(!ignorePerm)
                    this.Add(r.Name, r.Value, r.Permanent);
            }
        }*/
        
        public override string ToString()
        {
            if (this.Count == 0)
                return "";

            Debug.Separator();
            Debug.WriteLine("--- Replace List ---");

            foreach (Replace c in this)
                Debug.WriteLine(c.ToString());

            return "";
        }
    }
}
