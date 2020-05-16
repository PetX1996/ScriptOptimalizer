using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class Include
    {
    }

    class IncludeList : List<Reader>
    {
        public Reader Add(string file, int incDepth)
        {
            Reader r = Program.A_Readers.GetReader(file, incDepth);
            if (this.FindIndex(a => a.C_FileName == file) != -1)
            {
                Debug.Error("File already included \'" + file + "\' Line: " + Program.DEBUG_LineI);
                return null;
            }

            if (IsFileIncluded(this, file))
            {
                //Debug.Error("File already included 2 \'" + file + "\' Line: " + Program.DEBUG_LineI);
                return null;
            }

            //Debug.WriteLine("Adding include \'"+file+"\'");
            this.Add(r);
            return r;
        }

        private bool IsFileIncluded(IncludeList list, string file)
        {
            foreach (Reader r in list)
            {
                if (r.C_FileName == file)
                    return true;

                if (IsFileIncluded(r.LIST_Includes, file))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            if (this.Count == 0)
                return "";

            Debug.Separator();
            Debug.WriteLine("--- Include List ---");

            foreach (Reader r in this)
                Debug.WriteLine(r.C_FileName);

            return "";
        }
    }
}
