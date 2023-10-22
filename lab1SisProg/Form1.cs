using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab1SisProg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            exampleComboBox.SelectedIndex = 0;
            ExampleTKO();
        }

        readonly int columnsCount = 4;
        bool firstPass = false;
        int typeAdr = 0;
        FSPass fsp;
        DataCheck dC;

        private void FirstPassButton_Click(object sender, EventArgs e)
        {
            Clear();
            DeleteEmptyRows(operationCodeDataGrid);

            fsp = new FSPass();
            dC = new DataCheck();

            //помещаем ТКО в динамический массив
            string[,] operationCodeArray = new string[operationCodeDataGrid.RowCount - 1, operationCodeDataGrid.ColumnCount];

            for (int i = 0; i < operationCodeDataGrid.RowCount-1; i++)
                for (int j = 0; j < operationCodeDataGrid.ColumnCount; j++)
                    operationCodeArray[i, j] = Convert.ToString(operationCodeDataGrid.Rows[i].Cells[j].Value).ToUpper();

            //помещаем исходный код в динамический массив
            string[] str = sourceCodeTextBox.Text.Split('\n');

            for (int i = 0; i < str.Length; i++)
                str[i] = Convert.ToString(str[i]).Replace("\r", "");

            str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            
            string[,] sourceCodeArray = new string[str.Length, columnsCount];

            for (int i = 0; i < str.Length; i++)
                for (int j = 0; j < columnsCount; j++)
                    sourceCodeArray[i, j] = "";

            for (int i = 0; i < str.Length; i++)
            {
                str[i] = str[i].Trim();
                string[] temp = str[i].Split(' ');
                if (temp.Length >= 3)
                {
                    if (temp[2].IndexOf('"') == 1 && temp[temp.Length - 1].LastIndexOf('"') == temp[temp.Length - 1].Length - 1)
                    {
                        for (int j = 3; j < temp.Length; j++)
                        {
                            temp[2] += " " + temp[j];
                            temp[j] = "";
                        }
                    }
                    else if (temp[1].IndexOf('"') == 1 && (temp[temp.Length - 1].LastIndexOf('"') == temp[temp.Length - 1].Length - 1))
                    {
                        for (int j = 2; j < temp.Length; j++)
                        {
                            temp[1] += " " + temp[j];
                            temp[j] = "";
                        }
                    }
                }
                temp = temp.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                temp[temp.Length - 1] = temp[temp.Length - 1].Replace("\r", "");
                
                if (temp.Length <= 4)
                {
                    if (temp.Length == 1)
                    {
                        if (dC.CheckDirective(temp[0]) || fsp.FindCode(temp[0], operationCodeArray) != -1)
                            sourceCodeArray[i, 1] = temp[0];
                        else
                        {
                            for (int j = 0; j < temp.Length; j++)
                                sourceCodeArray[i, j] = temp[j];
                        }
                    }
                    else if(temp.Length == 2)
                    {
                        if (dC.CheckDirective(temp[0]) || fsp.FindCode(temp[0], operationCodeArray) != -1)
                        {
                            if (fsp.FindCode(temp[0], operationCodeArray) != -1 && typeAdr == 0 && (temp[1].Contains('[') || temp[1].Contains(']')))
                            {
                                MessageBox.Show($"Относительная адресация в {i + 1} строке. Выбрана прямая адресация.", "Внимание!");
                                return;
                            }
                            if(fsp.FindCode(temp[0], operationCodeArray) != -1 && typeAdr == 1 && (!temp[1].Contains('[') || !temp[1].Contains(']')) && !int.TryParse(temp[1], out int p))
                            {
                                MessageBox.Show($"Прямая адресация в {i + 1} строке. Выбрана относительная адресация.", "Внимание!");
                                return;
                            }
                            sourceCodeArray[i, 1] = temp[0];
                            sourceCodeArray[i, 2] = temp[1];
                        }
                        else if (dC.CheckDirective(temp[1]) || fsp.FindCode(temp[1], operationCodeArray) != -1)
                        {
                            sourceCodeArray[i, 0] = temp[0];
                            sourceCodeArray[i, 1] = temp[1];
                        }
                        else
                        {
                            for (int j = 0; j < temp.Length; j++)
                                sourceCodeArray[i, j] = temp[j];
                        }
                    }
                    else if(temp.Length == 3)
                    {
                        if (dC.CheckDirective(temp[0]) || fsp.FindCode(temp[0], operationCodeArray) != -1)
                        {
                            if (temp[2].IndexOf('"') == 1 && temp[3].IndexOf('"') == temp[3].Length - 1)
                            {
                                sourceCodeArray[i, 1] = temp[0];
                                sourceCodeArray[i, 2] = temp[1] + " " + temp[2];
                                sourceCodeArray[i, 3] = "";
                            }
                            else
                            {
                                sourceCodeArray[i, 1] = temp[0];
                                sourceCodeArray[i, 2] = temp[1];
                                sourceCodeArray[i, 3] = temp[2];
                            }
                        }
                        else if (dC.CheckDirective(temp[1]) || fsp.FindCode(temp[1], operationCodeArray) != -1)
                        {
                            if (fsp.FindCode(temp[1], operationCodeArray) != -1 && typeAdr == 0 && (temp[2].Contains('[') || temp[2].Contains(']')))
                            {
                                MessageBox.Show($"Относительная адресация в {i + 1} строке. Выбрана прямая адресация.", "Внимание!");
                                return;
                            }
                            if (fsp.FindCode(temp[1], operationCodeArray) != -1 && typeAdr == 1 && (!temp[2].Contains('[') || !temp[2].Contains(']')) && !int.TryParse(temp[2], out int p))
                            {
                                MessageBox.Show($"Прямая адресация в {i + 1} строке. Выбрана относительная адресация.", "Внимание!");
                                return;
                            }
                            sourceCodeArray[i, 0] = temp[0];
                            sourceCodeArray[i, 1] = temp[1];
                            sourceCodeArray[i, 2] = temp[2];
                        }
                        else
                        {
                            for (int j = 0; j < temp.Length; j++)
                                sourceCodeArray[i, j] = temp[j];
                        }
                    }
                    else if(temp.Length == 4)
                    {
                        if (dC.CheckDirective(temp[1]) || fsp.FindCode(temp[1], operationCodeArray) != -1)
                        {
                            if (fsp.FindCode(temp[1], operationCodeArray) != -1 && typeAdr == 0 && (temp[2].Contains('[') || temp[2].Contains(']')))
                            {
                                MessageBox.Show($"Относительная адресация в {i + 1} строке. Выбрана прямая адресация.", "Внимание!");
                                return;
                            }
                            if (fsp.FindCode(temp[1], operationCodeArray) != -1 && typeAdr == 1 && (!temp[2].Contains('[') || !temp[2].Contains(']')) && !int.TryParse(temp[2], out int p))
                            {
                                MessageBox.Show($"Прямая адресация в {i + 1} строке. Выбрана относительная адресация.", "Внимание!");
                                return;
                            }
                            if (temp[2].IndexOf('"') == 1 && temp[3].IndexOf('"') == temp[3].Length - 1)
                            {
                                sourceCodeArray[i, 0] = temp[0];
                                sourceCodeArray[i, 1] = temp[1];
                                sourceCodeArray[i, 2] = temp[2] + " " + temp[3];
                                sourceCodeArray[i, 3] = "";
                            }
                            else
                            {
                                sourceCodeArray[i, 0] = temp[0];
                                sourceCodeArray[i, 1] = temp[1];
                                sourceCodeArray[i, 2] = temp[2];
                                sourceCodeArray[i, 3] = temp[3];
                            }
                        }
                        else if (!(dC.CheckDirective(temp[1]) || fsp.FindCode(temp[1], operationCodeArray) != -1))
                        {
                            MessageBox.Show($"Синтаксическая ошибка в {i + 1} строке.", "Внимание!");
                            return;
                        }
                        else
                        {
                            for (int j = 0; j < temp.Length; j++)
                                sourceCodeArray[i, j] = temp[j];
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show($"Синтаксическая ошибка в {i + 1} строке. Элементов в строке больше 4", "Внимание!");
                    return;
                }
            }

            for (int i = 0; i < sourceCodeArray.GetLength(0); i++)
                sourceCodeArray[i, 1] = Convert.ToString(sourceCodeArray[i, 1]).ToUpper();

            for (int i = 0; i < str.Length; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                    Console.Write(sourceCodeArray[i, j]);
                Console.WriteLine();
            }


            if (fsp.CheckOperationCode(operationCodeArray))
            {
                if (fsp.DoFirstPass(sourceCodeArray, operationCodeArray, supportTableDataGrid, symbolTableDataGrid))
                {
                    firstPass = true;
                    WriteError(firstPassErrorTextBox, fsp.errorText);
                }
                else
                    WriteError(firstPassErrorTextBox, fsp.errorText);
            }
            else
                WriteError(firstPassErrorTextBox, fsp.errorText);


        }

        private void WriteError(TextBox textBox, string str)
        {
            textBox.Text += str + "\r\n";
        }

        private void Clear()
        {
            supportTableDataGrid.Rows.Clear();
            symbolTableDataGrid.Rows.Clear();
            firstPassErrorTextBox.Clear();
            binaryCodeTextBox.Clear();
            secondPassErrorTextBox.Clear();
            settingTableDataGrid.Rows.Clear();
        }

        private void SourceCodeTextBox_TextChanged(object sender, EventArgs e)
        {
            Clear();
            firstPass = false;
        }

        private void OperationCodeDataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            Clear();
            firstPass = false;
        }

        private void SecondPassButton_Click(object sender, EventArgs e)
        {
            binaryCodeTextBox.Clear();
            secondPassErrorTextBox.Clear();

            if (firstPass)
            {
                if (fsp.DoSecondPass(binaryCodeTextBox, settingTableDataGrid))
                {
                    WriteError(secondPassErrorTextBox, fsp.errorText);
                }
                else
                {
                    WriteError(secondPassErrorTextBox, fsp .errorText);
                }
            }
            else
            {
                MessageBox.Show($"Выполните первый проход!");
            }
        }

        public void DeleteEmptyRows(DataGridView DBGrid_source_code)
        {
            for (int i = 0; i < DBGrid_source_code.Rows.Count - 1; i++)
            {
                bool empty = true;
                for (int j = 0; j < DBGrid_source_code.Rows[i].Cells.Count; j++)
                    if ((DBGrid_source_code.Rows[i].Cells[j].Value != null) && (DBGrid_source_code.Rows[i].Cells[j].Value.ToString() != ""))
                        empty = false;

                if (empty)
                {
                    DBGrid_source_code.Rows.Remove(DBGrid_source_code.Rows[i]);
                }
            }
        }

        private void ExampleTKO()
        {
            operationCodeDataGrid.Rows.Add("JMP", "01", "4");
            operationCodeDataGrid.Rows.Add("LOADR1", "02", "4");
            operationCodeDataGrid.Rows.Add("LOADR2", "03", "4");
            operationCodeDataGrid.Rows.Add("ADD", "04", "2");
            operationCodeDataGrid.Rows.Add("SAVER1", "05", "4");
            operationCodeDataGrid.Rows.Add("NOP", "06", "1");
        }

        private void ExampleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clear();
            if (exampleComboBox.SelectedIndex == 0)
            {
                sourceCodeTextBox.Text = "prog start 0\r\njmp L1\r\nA1 RESB 10\r\nA2 RESW 20\r\nB1 WORD 4096\r\nB2 BYTE X\"2F4C008A\"\r\nB3 BYTE C\"Hello!\"\r\nB4 BYTE 128\r\nL1 LOADR1 B1\r\nLOADR2 B4\r\nADD R1 R2\r\nSAVER1 L1\r\nNOP\r\nEND 10";
                typeAdr = 0;
            }
            if (exampleComboBox.SelectedIndex == 1)
            {
                typeAdr = 1;
                sourceCodeTextBox.Text = "prog start 0\r\njmp [L1]\r\nA1 RESB 10\r\nA2 RESW 20\r\nB1 WORD 4096\r\nB2 BYTE X\"2F4C008A\"\r\nB3 BYTE C\"Hello!\"\r\nB4 BYTE 128\r\nL1 LOADR1 [B1]\r\nLOADR2 [B4]\r\nADD R1 R2\r\nSAVER1 [L1]\r\nNOP\r\nEND 10";
            }
            if (exampleComboBox.SelectedIndex == 2)
            {
                typeAdr = 2;
                sourceCodeTextBox.Text = "prog start 0\r\njmp [L1]\r\nA1 RESB 10\r\nA2 RESW 20\r\nB1 WORD 4096\r\nB2 BYTE X\"2F4C008A\"\r\nB3 BYTE C\"Hello!\"\r\nB4 BYTE 128\r\nL1 LOADR1 B1\r\nLOADR2 [B4]\r\nADD R1 R2\r\nSAVER1 [L1]\r\nNOP\r\nEND 10";
            }
        }
    }
}
