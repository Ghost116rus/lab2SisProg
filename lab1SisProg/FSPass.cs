using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace lab1SisProg
{
    public class FSPass : Pass
    {
        readonly DataCheck dC = new DataCheck();

        //Проверка ТКО
        public bool CheckOperationCode(string[,] OCA)
        {
            int rows = OCA.GetLength(0);

            for (int i = 0; i < rows; i++)
            {

                if (OCA[i,0] == "" || OCA[i,1] == "" || OCA[i,2] == "")
                {
                    errorText = $"В строке {i + 1} ошибка. Недопустима пустая ячейка в ТКО";
                    return false;
                }

                if (OCA[i,0].Length > 6 || OCA[i,1].Length > 2 || OCA[i,2].Length > 1)
                {
                    errorText = $"В строке {i + 1} ошибка. Недопустимый размер строки в ТКО. Команда - от 1 до 6. Код - от 1 до 2. Длина - не более одного";
                    return false;
                }

                if (!dC.CheckLettersAndNumbers(OCA[i,0]))
                {
                    errorText = $"В строке {i + 1} ошибка. Недопустимый символ в поле команды";
                    return false;
                }

                //Проверка на 16чные цифры
                if (dC.CheckAdress(OCA[i,1]))
                {
                    if (dC.CheckRegisters(OCA[i,0]) || dC.CheckDirective(OCA[i,0]))
                    {
                        errorText = $"В строке {i + 1} ошибка. Код команды является зарезервированным словом";
                        return false;
                    }

                    if (Converter.ConvertHexToDec(OCA[i,1]) > 63)
                    {
                        errorText = $"В строке {i + 1} ошибка. Код команды не должен превышать 3F";
                        return false;
                    }
                    else
                    {
                        if (OCA[i,1].Length == 1)
                            OCA[i,1] = Converter.ConvertToTwoChars(OCA[i,1]);
                    }
                }
                else
                {
                    errorText = $"В строке {i + 1} ошибка. Недопустимые символы в поле кода";
                    return false;
                }
                
                if (dC.CheckNumbers(OCA[i, 2]))
                {
                    int res = int.Parse(OCA[i, 2]);

                    /*if(res <= 0 || res > 4)
                    {
                        errorText = $"В строке {i + 1} ошибка. Недопустимый размер команды. Должен быть от 1 до 4";
                        return false;
                    }*/
                    if(res <= 0 || res > 4 || res == 3)
                    {
                        errorText = $"В строке {i + 1} ошибка. Недопустимый размер команды. Должен быть 1, 2 или 4";
                        return false;
                    }
                }
                else
                {
                    errorText = $"В строке {i + 1} ошибка. Недопустимые символы в поле размера операции";
                    return false;
                }

                
                for (int k = i + 1; k < rows; k++)
                {
                    string str1 = OCA[i, 0];
                    string str2 = OCA[k, 0];
                    if(Equals(str1, str2))
                    {
                        errorText = $"В строке {i + 1} ошибка. В поле команда найдены совпадения";
                        return false;
                    }
                }

                
                for (int k = i + 1; k < rows; k++)
                {
                    string str1 = Converter.ConvertHexToDec(OCA[i, 1]).ToString();
                    string str2 = Converter.ConvertHexToDec(OCA[k, 1]).ToString();
                    if (Equals(str1, str2))
                    {
                        errorText = $"В строке {i + 1} ошибка. В поле кода операции найдены совпадения";
                        return false;
                    }
                }
            }

            return true;
        }

        //Первый проход
        public bool DoFirstPass(string[,] sourceCode, string[,] operationCode, DataGridView supportTableDG, DataGridView symbolTableDG)
        {
            startAddress = 0;
            endAddress = 0;
            countAddress = 0;

            symbolTable.Add(new List<string>());
            symbolTable.Add(new List<string>());

            supportTable.Add(new List<string>());
            supportTable.Add(new List<string>());
            supportTable.Add(new List<string>());
            supportTable.Add(new List<string>());

            int numberRows = sourceCode.GetLength(0);

            bool flagStart = false;
            bool flagEnd = false;


            for (int i = 0; i < numberRows; i++)
            {
                if (flagStart)
                {
                    if (countAddress > memmoryMax)
                    {
                        errorText = $"В строке {i + 1} ошибка. Произошло переполнение";
                        return false;
                    }
                }

                if (flagEnd)
                    break;

                if (!dC.CheckRow(sourceCode, i, out string mark, out string OC, out string OP1, out string OP2, nameProg))
                {
                    errorText = $"В строке {i + 1} синтаксическая ошибка.";
                    return false;
                }

                if (FindMark(mark) != -1)
                {
                    errorText = $"В строке {i + 1} ошибка. Найдена уже существующая метка {mark}";
                    return false;
                }
                else
                {
                    if (mark != "" && flagStart)
                    {
                        symbolTable[0].Add(mark);
                        symbolTable[1].Add(Converter.ConvertToSixChars(Converter.ConvertDecToHex(countAddress)));
                    }

                    if (dC.CheckDirective(OC))
                    {
                        switch (OC)
                        {
                            case "START":
                                {
                                    if (i == 0 && !flagStart)
                                    {
                                        flagStart = true;

                                        if (dC.CheckAdress(OP1) || OP1 == "")
                                        {
                                            OP1 = OP1.TrimStart('0');
                                            
                                            if (OP1 == "")
                                                OP1 = "0";

                                            countAddress = Converter.ConvertHexToDec(OP1);

                                            if(countAddress > 0)
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Адрес начала программы должен быть равен нулю";
                                                return false;
                                            }

                                            startAddress = countAddress;

                                            if (countAddress > memmoryMax || countAddress < 0)
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Неправильный адрес загрузки";
                                                return false;
                                            }

                                            if (mark == "")
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Не задано имя программы";
                                                return false;
                                            }

                                            if (mark.Length > 10)
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Первышена длина имени программы\n Имя программы должно быть не больше 10 символов";
                                                return false;
                                            }

                                            AddToSupportTable(mark, OC, Converter.ConvertToSixChars(OP1), "");
                                            nameProg = mark;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд директивы START не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Неверный адрес начала программы";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        errorText = $"В строке {i + 1} ошибка. Повторное использование директивы START";
                                        return false;
                                    }
                                }
                                break;

                            case "WORD":
                                {
                                    if (int.TryParse(OP1, out int numb))
                                    {
                                        if (numb >= 0 && numb <= memmoryMax)
                                        {
                                            if (!AddCheckError(i, 3, OC, Convert.ToString(numb), ""))
                                                return false;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд директивы WORD не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Отрицательное число либо превышено максимальное значение числа";
                                            return false;
                                        }
                                    }
                                    /*else
                                    {
                                        if (OP1.Length == 1 && OP1 == "?")
                                        {
                                            if (!AddCheckError(i, 3, OC, Convert.ToString(numb), ""))
                                                return false;

                                            if (OP2.Length > 0)
                                                errorText = $"В строке {i + 1} второй операнд директивы WORD не рассматривается";
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Отрицательное число либо превышено максимальное значение числа";
                                            return false;
                                        }
                                    }*/
                                }
                                break;

                            case "BYTE":
                                {

                                    if (int.TryParse(OP1, out int numb))
                                    {
                                        if (numb >= 0 && numb <= 255)
                                        {
                                            if (!AddCheckError(i, 1, OC, Convert.ToString(numb), ""))
                                                return false;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд директивы BYTE не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Отрицательное число либо превышено максимальное значение числа";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        string symb = dC.CheckAndGetString(OP1);

                                        if (symb != "")
                                        {
                                            if (!AddCheckError(i, symb.Length, OC, OP1, ""))
                                                return false;

                                            if (OP2.Length > 0) { 
                                                errorText = $"В строке {i + 1} второй операнд директивы BYTE не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }

                                            continue;
                                        }

                                        symb = dC.CheckAndGetByteString(OP1);

                                        if (symb != "")
                                        {
                                            if (symb.Length % 2 == 0)
                                            {
                                                if (!AddCheckError(i, symb.Length / 2, OC, OP1, ""))
                                                    return false;

                                                if (OP2.Length > 0)
                                                {
                                                    errorText = $"В строке {i + 1} второй операнд директивы BYTE не рассматривается. Устраните и повторите заново.";
                                                    return false;
                                                }

                                                continue;
                                            }
                                            else
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Невозможно преобразовать BYTE нечетное количество символов";
                                                return false;
                                            }
                                        }

                                        /*if (OP1.Length == 1 && OP1 == "?")
                                        {
                                            if (!AddCheckError(i, 1, OC, OP1, ""))
                                                return false;

                                            if (OP2.Length > 0)
                                                errorText = $"В строке {i + 1} второй операнд директивы BYTE не рассматривается";

                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Неверный формат строки {OP1}";
                                            return false;
                                        }*/

                                        errorText = $"В строке {i + 1} ошибка. Неверный формат строки {OP1}";
                                        return false;
                                    }
                                }
                                break;

                            case "RESB":
                                {
                                    if (int.TryParse(OP1, out int numb))
                                    {
                                        if (numb > 0)
                                        {
                                            if (!AddCheckError(i, numb, OC, Convert.ToString(numb), ""))
                                                return false;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд директивы RESB не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Количество байт равно нулю или меньше нуля";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        errorText = $"В строке {i + 1} ошибка. Невозможно выполнить преобразование {OP1}";
                                        return false;
                                    }
                                }
                                break;

                            case "RESW":
                                {
                                    if (int.TryParse(OP1, out int numb))
                                    {
                                        if (numb > 0)
                                        {
                                            if (!AddCheckError(i, numb * 3, OC, Convert.ToString(numb), ""))
                                                return false;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд директивы RESW не рассматривается. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Количество байт равно нулю или меньше нуля";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        errorText = $"В строке {i + 1} ошибка. Невозможно выполнить преобразование {OP1}";
                                        return false;
                                    }
                                }
                                break;

                            case "END":
                                {
                                    if (mark.Length > 0)
                                    {
                                        errorText = $"В строке {i + 1} метка. Устраните и повторите заново.";
                                        return false;
                                    }

                                    if (flagStart && !flagEnd)
                                    {
                                        flagEnd = true;
                                        if (OP1.Length == 0)
                                        {
                                            endAddress = startAddress;
                                            AddToSupportTable(Converter.ConvertToSixChars(Converter.ConvertDecToHex(countAddress)), OC, Convert.ToString("0"), "");
                                        }
                                        else
                                        {
                                            if (dC.CheckAdress(OP1))
                                            {
                                                endAddress = Converter.ConvertHexToDec(OP1);
                                                if (endAddress >= startAddress && endAddress <= countAddress)
                                                {
                                                    AddToSupportTable(Converter.ConvertToSixChars(Converter.ConvertDecToHex(countAddress)), OC, Convert.ToString(OP1), "");
                                                    break;
                                                }
                                                else
                                                {
                                                    errorText = $"В строке {i + 1} ошибка. Неверный адрес входа в программу";
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                errorText = $"В строке {i + 1} ошибка. Неверный адрес входа в программу";
                                                return false;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (OC.Length > 0)
                        {
                            int numb = FindCode(OC, operationCode);
                            if (numb > -1)
                            {
                                if (operationCode[numb, 2] == "1")
                                {
                                    if (!AddCheckError(i, 1, Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Converter.ConvertHexToDec(operationCode[numb, 1]) * 4)), "", ""))
                                        return false;

                                    if (OP1.Length > 0 || OP2.Length > 0)
                                    {
                                        errorText = $"В строке {i + 1} операнды не рассматривается в команде {operationCode[numb, 0]}. Устраните и повторите заново.";
                                        return false;
                                    }
                                }

                                else if (operationCode[numb, 2] == "2")
                                {
                                    if (int.TryParse(OP1, out int number))
                                    {

                                        if (number >= 0 && number <= 255)
                                        {
                                            if (!AddCheckError(i, 2, Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Converter.ConvertHexToDec(operationCode[numb, 1]) * 4)), OP1, ""))
                                                return false;

                                            if (OP2.Length > 0)
                                            {
                                                errorText = $"В строке {i + 1} второй операнд не рассматривается в команде {operationCode[numb, 0]}. Устраните и повторите заново.";
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Отрицательное число либо превышено максимальное значение числа";
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (dC.CheckRegisters(OP1) && dC.CheckRegisters(OP2))
                                        {
                                            if (!AddCheckError(i, 2, Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Converter.ConvertHexToDec(operationCode[numb, 1]) * 4)), OP1, OP2))
                                                return false;
                                        }
                                        else
                                        {
                                            errorText = $"В строке {i + 1} ошибка. Ошибка в команде {operationCode[numb, 0]}";
                                            return false;
                                        }
                                    }
                                }
                                /*else if (operationCode[numb, 2] == "3")
                                {
                                    if (!AddCheckError(i, 3, Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Converter.ConvertHexToDec(operationCode[numb, 1]) * 4 + 1)), OP1, OP2))
                                        return false;

                                    if (OP2.Length > 0)
                                        errorText = $"В строке {i + 1} второй операнд не рассматривается в команде {operationCode[numb, 0]}";
                                }*/
                                else if (operationCode[numb, 2] == "4")
                                {
                                    int number;
                                    if (OP1.Length > 0)
                                    {
                                        if (OP1[0] == '[' && OP1[OP1.Length - 1] == ']')
                                            number = Converter.ConvertHexToDec(operationCode[numb, 1]) * 4 + 2;
                                        else
                                            number = Converter.ConvertHexToDec(operationCode[numb, 1]) * 4 + 1;
                                    }
                                    else
                                    {
                                        errorText = $"В строке {i + 1} ошибка. Не найден операнд";
                                        return false;
                                    }

                                    if (!AddCheckError(i, 4, Converter.ConvertToTwoChars(Converter.ConvertDecToHex(number)), OP1, OP2))
                                        return false;

                                    if (OP2.Length > 0)
                                    {
                                        errorText = $"В строке {i + 1} второй операнд не рассматривается в команде {operationCode[numb, 0]}. Устраните и повторите заново.";
                                        return false;
                                    }
                                }
                                else
                                {
                                    errorText = $"В строке {i + 1} размер команды больше установленного";
                                    return false;
                                }
                            }
                            else
                            {
                                errorText = $"В строке {i + 1} ошибка. В ТКО не найдено {OC}";
                                return false;
                            }
                        }
                        else
                        {
                            errorText = $"В строке {i + 1} ошибка. Синтаксическая ошибка";
                            return false;
                        }
                    }
                }
            }

            if (!flagEnd)
            {
                errorText = $"Не найдена точка входа в программу";
                return false;
            }

            for (int i = 0; i < supportTable[0].Count; i++)
            {
                supportTableDG.Rows.Add();
                supportTableDG.Rows[i].Cells[0].Value = supportTable[0][i];
                supportTableDG.Rows[i].Cells[1].Value = supportTable[1][i];
                supportTableDG.Rows[i].Cells[2].Value = supportTable[2][i];
                supportTableDG.Rows[i].Cells[3].Value = supportTable[3][i];
            }
            
            for (int i = 0; i < symbolTable[1].Count; i++)
            {
                symbolTableDG.Rows.Add();
                symbolTableDG.Rows[i].Cells[0].Value = symbolTable[0][i];
                symbolTableDG.Rows[i].Cells[1].Value = symbolTable[1][i];
            }

            return true;
        }

        private bool AddCheckError(int i, int numbToAdd, string OC, string OP1, string OP2)
        {
            if (countAddress + numbToAdd > memmoryMax)
            {
                errorText = $"В строке {i + 1} ошибка. Произошло переполнение";
                return false;
            }

            AddToSupportTable(Converter.ConvertToSixChars(Converter.ConvertDecToHex(countAddress)),OC, OP1, OP2);

            countAddress += numbToAdd;

            if (!CheckMemmory())
                return false;

            return true;
        }

        public bool DoSecondPass(TextBox BC, DataGridView settingTableDG)
        {
            errorText = "";

            List<string> settingTable = new List<string>();

            for (int i = 0; i < supportTable[0].Count; i++)
            {
                string address = supportTable[0][i];
                string OC = supportTable[1][i];
                string OP1 = supportTable[2][i];
                string OP2 = supportTable[3][i];

                if(i == 0)
                    BC.Text = Converter.ConvertToBinaryCodeSTART(address, OP1, Convert.ToString(countAddress - startAddress)) + "\r\n";
                else
                {
                    string res = CheckOP(OP1, out bool error, out bool flagMark, out string tuneAddress, i);

                    if (error)
                    {
                        errorText = $"В строке {i + 1} ошибка. Код операнда отсутствует в ТСИ.";
                        BC.Clear();
                        break;
                    }

                    SettingTable(tuneAddress, settingTable);

                    string ress = CheckOP(OP2, out error, out _, out tuneAddress, i);

                    if (error)
                    {
                        errorText = $"В строке {i + 1} ошибка. Код операнда отсутствует в ТСИ.";
                        BC.Clear();
                        break;
                    }

                    SettingTable(tuneAddress, settingTable);

                    if (dC.CheckDirective(OC))
                    {
                        if(OC == "RESB")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", res, "", "") + "\r\n";
                            continue;
                        }
                        else if (OC == "RESW")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Convert.ToInt32(OP1) * 3)), "", "") + "\r\n";
                            continue;
                        }
                        else if (OC == "BYTE")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", Converter.ConvertToTwoChars(Converter.ConvertDecToHex(res.Length + ress.Length)), res, ress) + "\r\n";
                            continue;
                        }
                        else if (OC == "WORD")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", Converter.ConvertToTwoChars(Converter.ConvertDecToHex(Converter.ConvertToSixChars(res).Length + ress.Length)), Converter.ConvertToSixChars(res), ress) + "\r\n";
                            continue;
                        }
                        /*else if (OC == "BYTE" && OP1 == "?")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", Converter.ConvertToTwoChars(Converter.ConvertDecToHex(1)), "", "") + "\r\n";
                            continue;
                        }
                        else if (OC == "WORD" && OP1 == "?")
                        {
                            BC.Text += Converter.ConvertToBinaryCode(address, "", Converter.ConvertToTwoChars(Converter.ConvertDecToHex(3)), "", "") + "\r\n";
                            continue;
                        }*/
                    }
                    else
                    {
                        int type = (byte)Converter.ConvertHexToDec(OC) & 0x03;
                        if(type == 1)
                        {
                            if (!flagMark)
                            {
                                errorText = $"В строке {i + 1} ошибка. Для данного типа адресации операнд должен быть меткой";
                                BC.Clear();
                                return false;
                            }
                            if(ress != "")
                            {
                                errorText = $"В строке {i + 1} ошибка. Данный тип адрессации поддерживает один операнд";
                                BC.Clear();
                                return false;
                            }
                        }

                        BC.Text += Converter.ConvertToBinaryCode(address, OC,
                            Converter.ConvertToTwoChars(Converter.ConvertDecToHex(OC.Length + res.Length + ress.Length)), res, ress) + "\r\n";
                    }
                }
                if (settingTable.Count > 0)
                {
                    settingTableDG.Rows.Clear();
                    for (int j = 0; j < settingTable.Count; j++)
                        settingTableDG.Rows.Add(settingTable[j]);
                }
            }

            for (int j = 0; j < settingTable.Count; j++)
                BC.Text += Converter.ConvertToBinaryCodeSetting(settingTable[j]) + "\r\n";

            BC.Text += Converter.ConvertToBinaryCodeEND(Converter.ConvertToSixChars(Converter.ConvertDecToHex(endAddress))) + "\r\n";

            if (errorText != "")
                BC.Clear();

            return true;
        }

        public string CheckOP(string OP, out bool er, out bool flag, out string adress, int ind)
        {
            er = false;
            flag = false;
            adress = "";
            if(OP != "")
            {
                int n = FindMark(OP);
                if (n > -1)
                {
                    flag = true;
                    adress = supportTable[0][ind];
                    return symbolTable[1][n];
                }
                else if (OP[0] == '[' && OP[OP.Length - 1] == ']')
                {
                    string temp = OP;
                    temp = temp.Substring(1, temp.Length - 2);
                    n = FindMark(temp);
                    if (n > -1)
                    {
                        flag = true; 
                        return Converter.ConvertSubHex(symbolTable[1][n], supportTable[0][ind + 1]);
                    }
                    er = true;
                }
                else
                {
                    int reg = dC.GetRegisters(OP);
                    int maxValue = (int)Math.Pow(2, 31) - 1;
                    if (reg > -1)
                        return Converter.ConvertDecToHex(reg);
                    else
                    {
                        if (dC.CheckNumbers(OP))
                        {
                            if (Convert.ToDouble(OP) > maxValue || Convert.ToDouble(OP) < (maxValue * -1) + 1)
                            {
                                er = true;
                            }
                            else
                                return Converter.ConvertDecToHex(Convert.ToInt32(OP));
                        }
                        else
                        {
                            string str = dC.CheckAndGetString(OP);

                            if (str != "")
                                return Converter.ConvertASCII(str);

                            str = dC.CheckAndGetByteString(OP);

                            if (str != "")
                                return str;

                            er = true;
                        }
                    }
                }
            }
            //er = true;
            return "";
        }

        public bool SettingTable(string adr, List<string> settingTable)
        {
            if (adr.Length > 0)
            {
                if (settingTable.FindIndex(x => x == adr) > 0)
                    return false;
                else
                    settingTable.Add(adr);
            }
            return true;
        }
    }
}
