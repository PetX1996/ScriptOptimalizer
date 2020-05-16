using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptOptimalizer
{
    class Math
    {
        public static string CalculateString(string str)
        {
            while (true)
            {
                int bracketI = str.LastIndexOf('(');
                int bracketEndI = str.IndexOf(')');
                if (bracketI == -1 && bracketEndI == -1)
                    break;
                else if (bracketI == -1 || bracketEndI == -1)
                    throw new FormatException("Bad Syntax '(' ')' Line: "+Program.DEBUG_LineI);

                string bracketContent = str.Substring(bracketI + 1, bracketEndI - bracketI - 1);
                str = str.Replace("("+bracketContent+")", CalculateString(bracketContent));
            }

            int indexer = 0;
            List<MathOperand> operands = new List<MathOperand>();
            List<MathOperator> operators = new List<MathOperator>();
            string number = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '.' || (str[i] >= '0' && str[i] <= '9'))
                    number += str[i].ToString();
                else
                {
                    if (number != "")
                    {
                        operands.Add(new MathOperand(number, indexer));
                        indexer++;
                        number = "";
                    }

                    operators.Add(new MathOperator(str[i], indexer));
                    indexer++;
                }
            }

            foreach (MathOperator o in operators.Where(o => o.Operand == MathOperator.Operands.Multiplication))
            {
                MathOperand first = operands.Find(p => p.Index == o.Index - 1);
                MathOperand second = operands.Find(q => q.Index == o.Index + 1);

                first.Value *= second.Value;

                foreach (MathOperator op in operators)
                { 
                    if ( op.Index > o.Index )
                        op.Index -= 2;
                }
                operators.Remove(o);
                foreach (MathOperand od in operands)
                {
                    if (od.Index > o.Index)
                        od.Index -= 2;
                }
                operands.Remove(second);
            }

            return operands[0].Value.ToString();
        }
    }

    class MathOperand
    {
        public float Value { get; set; }
        public int Index { get; set; }
        public MathOperand(string number, int index)
        {
            this.Value = Convert.ToSingle(number);
            this.Index = index;
        }
    }

    class MathOperator
    {
        public enum Operands
        { 
            Multiplication,
            Division,
            Addition,
            Substraction
        }

        public Operands Operand { get; set; }
        public int Index { get; set; }
        public MathOperator(char operand, int index)
        {
            switch (operand)
            {
                case '*':
                    this.Operand = Operands.Multiplication;
                    break;
                case '/':
                    this.Operand = Operands.Division;
                    break;
                case '+':
                    this.Operand = Operands.Addition;
                    break;
                case '-':
                    this.Operand = Operands.Substraction;
                    break;
                default:
                    break;
            }

            this.Index = index;
        }
    }
}
