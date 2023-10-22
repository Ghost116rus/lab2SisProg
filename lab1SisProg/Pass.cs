using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1SisProg
{
    public class Pass
    {
        public string errorText = "";
        public string nameProg;
        public int startAddress = 0;
        public int endAddress = 0;
        public int countAddress = 0;
        public const int memmoryMax = 33554432;

        public List<List<string>> supportTable = new List<List<string>>();
        public List<List<string>> symbolTable = new List<List<string>>();

        public int FindMark(string mark)
        {
            for (int i = 0; i < symbolTable[0].Count; i++)
                if (mark == symbolTable[0][i])
                    return i;
            return -1;
        }

        public void AddToSupportTable(string mark, string OC, string OP1, string OP2)
        {
            supportTable[0].Add(mark);
            supportTable[1].Add(OC);
            supportTable[2].Add(OP1);
            supportTable[3].Add(OP2);
        }

        public bool CheckMemmory()
        {
            if (countAddress < 0 || countAddress > memmoryMax)
            {
                errorText = $"Ошибка. Выход за пределы доступной памяти";
                return false;
            }
            return true;
        }

        public int FindCode(string mark, string[,] operationCode)
        {
            for (int i = 0; i < operationCode.GetLength(0); i++)
            {
                if (Convert.ToString(mark).ToUpper() == operationCode[i, 0])
                    return i;
            }
            return -1;
        }
    }
}
