using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SatisfiabilityChecker
{

    class Checker
    {
        public bool IsSpecialSymbol(char c)
        {

            if (c != '(' && c != ')' && c != '*' && c != '!' && c != '+' && c != '>' && c != '=')
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void FindOperands(ref string operationString, int operationIndex, string operandsValues, bool[] intermediateResults, string previousOperationsRedults, int operationsAmount, ref bool operand1, ref bool operand2, bool isOperationDouble = false)
        {
            int f = operationIndex - 1;
            int s = (isOperationDouble) ? operationIndex + 2 : operationIndex + 1;
            while ((f > 0) && (!IsSpecialSymbol(operationString[f - 1]))) f--;
            while ((s < operationString.Length - 1) && (!IsSpecialSymbol(operationString[s + 1]))) s++;
            string op1 = operationString.Substring(f, operationIndex - f);
            string op2 = operationString.Substring((isOperationDouble) ? operationIndex + 2 : operationIndex + 1, (isOperationDouble) ? s - operationIndex - 1 : s - operationIndex).Replace("!", "");
            if (op1[0] == '{')
            {
                operand1 = intermediateResults[Convert.ToInt32(op1.Replace("{", "").Replace("}", ""))];
            }
            else if (op1[0] == '[')
            {
                operand1 = (previousOperationsRedults[Convert.ToInt32(op1.Replace("[", "").Replace("]", ""))] == '1');
            }
            else
            {
                operand1 = (operandsValues[Convert.ToInt32(op1.Replace("{", "").Replace("}", ""))] == '1');
                if ((f > 0) && (operationString[f - 1] == '!'))
                {
                    operand1 = !operand1;
                }
            }
            if (isOperationDouble) operationIndex++;
            if (op2[0] == '{')
            {
                operand2 = intermediateResults[Convert.ToInt32(op2.Replace("{", "").Replace("}", ""))];
            }
            else if (op2[0] == '[')
            {
                operand2 = (previousOperationsRedults[Convert.ToInt32(op2.Replace("[", "").Replace("]", ""))] == '1');
            }
            else
            {
                operand2 = (operandsValues[Convert.ToInt32(op2.Replace("{", "").Replace("}", ""))] == '1');
                if ((operationString[operationIndex + 1] == '!'))
                {
                    operand2 = !operand2;
                }
            }
            operationString = operationString.Replace(operationString.Substring(f, s - f + 1), "[" + operationsAmount.ToString() + "]");
            if (operationString[0] == '!')
                operationString = operationString.Remove(0, 1);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string strInput = "", inputVector = "";
            Checker checker = new Checker();
            int separateOperationsCount = 0, variablesCounter = 0;
            List<string> operationsStrKeeper = new List<string>();
            List<string> statementVariables = new List<string>();
            Console.WriteLine("Enter the compound proposition line:");
            strInput = Console.ReadLine().Replace(" ", "");
            Stopwatch stopWatch = Stopwatch.StartNew();
            int i = 0;
            while (i < strInput.Length)
            {
                if (!checker.IsSpecialSymbol(strInput[i]) && !Char.IsDigit(strInput[i]))
                {
                    int j = i;
                    while (j < strInput.Length - 1 && !checker.IsSpecialSymbol(strInput[j + 1]))
                    {
                        j++;
                    }
                    statementVariables.Add(strInput.Substring(i, j - i + 1));
                    strInput = strInput.Replace(strInput.Substring(i, j - i + 1), variablesCounter.ToString());
                    variablesCounter++;
                }
                i++;
            }
            i = 0;
            while (i < strInput.Length)
            {
                if (strInput[i] == ')')
                {
                    int j = i;
                    do
                    {
                        j--;
                        //Console.WriteLine(strInput[j]);
                    } while (j >= 0 && strInput[j] != '(');
                    if (j < 0)
                    {
                        Console.WriteLine("Input error");
                        Console.ReadKey();
                        return;
                    }
                    operationsStrKeeper.Add(strInput.Substring(j + 1, i - j - 1));
                    strInput = strInput.Replace(strInput.Substring(j, i - j + 1), "{" + separateOperationsCount.ToString() + "}");
                    separateOperationsCount++;
                    i = j;
                }
                i++;
            }
            operationsStrKeeper.Add(strInput);
            string[] operationsArray = operationsStrKeeper.ToArray();
            bool[] intermediateResults = new bool[separateOperationsCount];
            bool firstOperand = false, secondOperand = false, isSatisfiable = false;
            for (long l = 0; l < ((long)2 << variablesCounter - 1); l++)
            {
                inputVector = Convert.ToString(l, 2).PadLeft(variablesCounter, '0');
                int performedOperationsAmount = 0;
                string previousOperationsResults = "";
                operationsArray = operationsStrKeeper.ToArray();
                for (int j = 0; j <= separateOperationsCount; j++)
                {
                    bool lastResult = false, wasPerformed = false;
                    while (operationsArray[j].Contains('*'))
                    {
                        int k = operationsArray[j].IndexOf('*');
                        checker.FindOperands(ref operationsArray[j], k, inputVector, intermediateResults, previousOperationsResults, performedOperationsAmount, ref firstOperand, ref secondOperand);
                        previousOperationsResults += (firstOperand && secondOperand) ? '1' : '0';
                        lastResult = (firstOperand && secondOperand);
                        performedOperationsAmount++;
                        wasPerformed = true;
                    }
                    while (operationsArray[j].Contains('+'))
                    {
                        int k = operationsArray[j].IndexOf('+');
                        checker.FindOperands(ref operationsArray[j], k, inputVector, intermediateResults, previousOperationsResults, performedOperationsAmount, ref firstOperand, ref secondOperand);
                        previousOperationsResults += (firstOperand || secondOperand) ? '1' : '0';
                        lastResult = (firstOperand || secondOperand);
                        performedOperationsAmount++;
                        wasPerformed = true;
                    }
                    while (operationsArray[j].Contains('>'))
                    {
                        int k = operationsArray[j].IndexOf('>');
                        checker.FindOperands(ref operationsArray[j], k, inputVector, intermediateResults, previousOperationsResults, performedOperationsAmount, ref firstOperand, ref secondOperand);
                        previousOperationsResults += (!firstOperand || secondOperand) ? '1' : '0';
                        lastResult = (!firstOperand || secondOperand);
                        performedOperationsAmount++;
                        wasPerformed = true;
                    }
                    while (operationsArray[j].Contains("=="))
                    {
                        int k = operationsArray[j].IndexOf("==");
                        checker.FindOperands(ref operationsArray[j], k, inputVector, intermediateResults, previousOperationsResults, performedOperationsAmount, ref firstOperand, ref secondOperand, true);
                        previousOperationsResults += (firstOperand == secondOperand) ? '1' : '0';
                        lastResult = (firstOperand == secondOperand);
                        performedOperationsAmount++;
                        wasPerformed = true;
                    }
                    while (operationsArray[j].Contains("!="))
                    {
                        int k = operationsArray[j].IndexOf("!=");
                        checker.FindOperands(ref operationsArray[j], k, inputVector, intermediateResults, previousOperationsResults, performedOperationsAmount, ref firstOperand, ref secondOperand, true);
                        previousOperationsResults += (firstOperand != secondOperand) ? '1' : '0';
                        lastResult = (firstOperand != secondOperand);
                        performedOperationsAmount++;
                        wasPerformed = true;
                    }
                    if (j < operationsArray.Length - 1)
                    {
                        intermediateResults[j] = lastResult;
                    }
                    else
                    {
                        if (!wasPerformed)
                        {
                            lastResult = (intermediateResults[operationsArray.Length - 2]);
                        }
                        if (lastResult)
                        {
                            Console.WriteLine("Statement is satisfiable when: ");
                            string[] variableNames = statementVariables.ToArray();
                            for (int k = 0; k < variableNames.Length; k++)
                            {
                                Console.WriteLine(variableNames[k] + " = " + (inputVector[k] == '1'));
                            }
                            isSatisfiable = true;
                            break;
                        }
                    }
                }
                if (isSatisfiable) break;
            }
            stopWatch.Stop();
            long miliseconds = stopWatch.ElapsedMilliseconds;
            if (!isSatisfiable)
            {
                Console.WriteLine("This statement is not satisfiable\nMilliseconds passed: " + miliseconds.ToString());
            }
            else
            {
                Console.WriteLine("Milliseconds passed: " + miliseconds.ToString());
            }
            Console.ReadKey();
        }
    }
}
