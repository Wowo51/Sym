//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;

namespace Sym
{
    public enum OperatorType { Delimiting, Enclosing, Unary }
    public class Operator
    {
        public string OperatorString;
        public int OperatorIndex;
        public OperatorType OperatorType;
        public Func<List<double>, double> Function;

        public static List<Operator> BuildOperators()
        {
            List<Operator> operators = new List<Operator>();
            operators.Add(new Operator(",", 0, OperatorType.Delimiting, null));
            operators.Add(new Operator("=", 1, OperatorType.Delimiting, null));
            operators.Add(new Operator("+", 2, OperatorType.Delimiting, AddFunction));
            operators.Add(new Operator("-", 3, OperatorType.Delimiting, SubtractFunction));
            operators.Add(new Operator("*", 4, OperatorType.Delimiting, MultiplyFunction));
            operators.Add(new Operator("/", 5, OperatorType.Delimiting, DivideFunction));
            operators.Add(new Operator("-", 6, OperatorType.Unary, NegateFunction));
            operators.Add(new Operator("", 7, OperatorType.Enclosing, ParenthesesFunction));
            operators.Add(new Operator("Sin", 8, OperatorType.Enclosing, SinFunction));
            operators.Add(new Operator("Cos", 9, OperatorType.Enclosing, CosFunction));
            operators.Add(new Operator("Tan", 10, OperatorType.Enclosing, TanFunction));
            operators.Add(new Operator("Asin", 11, OperatorType.Enclosing, AsinFunction));
            operators.Add(new Operator("Acos", 12, OperatorType.Enclosing, AcosFunction));
            operators.Add(new Operator("Atan", 13, OperatorType.Enclosing, AtanFunction));
            operators.Add(new Operator("Pow", 14, OperatorType.Enclosing, PowFunction));
            operators.Add(new Operator("Sign", 15, OperatorType.Enclosing, SignFunction));
            operators.Add(new Operator("Abs", 16, OperatorType.Enclosing, AbsFunction));
            operators.Add(new Operator("Sqrt", 17, OperatorType.Enclosing, SqrtFunction));

            //These should be moved to SymAI.Regression
            //operators.Add(new Operator("IfEquals", 18, OperatorType.Enclosing, IfEqualsFunction));
            //operators.Add(new Operator("IfLess", 19, OperatorType.Enclosing, IfLessFunction));
            //operators.Add(new Operator("IfLessOrEquals", 20, OperatorType.Enclosing, IfLessOrEqualsFunction));
            //operators.Add(new Operator("IfZero", 21, OperatorType.Enclosing, IfZeroFunction));
            //operators.Add(new Operator("IfOne", 22, OperatorType.Enclosing, IfOneFunction));

            return operators;
        }

        public Operator(string operatorString, int operatorIndex, OperatorType operatorType, Func<List<double>, double> function)
        {
            OperatorString = operatorString;
            OperatorIndex = operatorIndex;
            OperatorType = operatorType;
            Function = function;
        }

        public static double AddFunction(List<double> inNumbers)
        {
            double tally = inNumbers[0];
            for (int i = 1; i < inNumbers.Count; i++)
            {
                tally += inNumbers[i];
            }
            return tally;
        }

        public static double SubtractFunction(List<double> inNumbers)
        {
            double tally = inNumbers[0];
            for (int i = 1; i < inNumbers.Count; i++)
            {
                tally -= inNumbers[i];
            }
            return tally;
        }

        public static double MultiplyFunction(List<double> inNumbers)
        {
            double tally = inNumbers[0];
            for (int i = 1; i < inNumbers.Count; i++)
            {
                tally *= inNumbers[i];
            }
            return tally;
        }

        public static double DivideFunction(List<double> inNumbers)
        {
            double tally = inNumbers[0];
            for (int i = 1; i < inNumbers.Count; i++)
            {
                tally /= inNumbers[i];
            }
            return tally;
        }

        public static double SinFunction(List<double> inNumbers)
        {
            return Math.Sin(inNumbers[0]);
        }

        public static double CosFunction(List<double> inNumbers)
        {
            return Math.Cos(inNumbers[0]);
        }

        public static double TanFunction(List<double> inNumbers)
        {
            return Math.Tan(inNumbers[0]);
        }

        public static double AsinFunction(List<double> inNumbers)
        {
            return Math.Asin(inNumbers[0]);
        }

        public static double AcosFunction(List<double> inNumbers)
        {
            return Math.Acos(inNumbers[0]);
        }

        public static double AtanFunction(List<double> inNumbers)
        {
            return Math.Atan(inNumbers[0]);
        }

        public static double PowFunction(List<double> inNumbers)
        {
            return Math.Pow(inNumbers[0], inNumbers[1]);
        }

        public static double AbsFunction(List<double> inNumbers)
        {
            return Math.Abs(inNumbers[0]);
        }

        public static double SignFunction(List<double> inNumbers)
        {
            if (double.IsNaN(inNumbers[0]))
            {
                return double.NaN;
            }
            return Math.Sign(inNumbers[0]);
        }

        public static double SqrtFunction(List<double> inNumbers)
        {
            return Math.Sqrt(inNumbers[0]);
        }

        public static double NegateFunction(List<double> inNumbers)
        {
            return -1 * inNumbers[0];
        }

        public static double ParenthesesFunction(List<double> inNumbers)
        {
            return inNumbers[0];
        }

        public static double IfEqualsFunction(List<double> inNumbers)
        {
            if (inNumbers[0] == inNumbers[1])
            {
                return inNumbers[2];
            }
            else
            {
                return inNumbers[3];
            }
        }

        public static double IfLessFunction(List<double> inNumbers)
        {
            if (inNumbers[0] < inNumbers[1])
            {
                return inNumbers[2];
            }
            else
            {
                return inNumbers[3];
            }
        }

        public static double IfLessOrEqualsFunction(List<double> inNumbers)
        {
            if (inNumbers[0] <= inNumbers[1])
            {
                return inNumbers[2];
            }
            else
            {
                return inNumbers[3];
            }
        }

        public static double IfZeroFunction(List<double> inNumbers)
        {
            if (inNumbers[0] == 0)
            {
                return inNumbers[1];
            }
            else
            {
                return inNumbers[2];
            }
        }

        public static double IfOneFunction(List<double> inNumbers)
        {
            if (inNumbers[0] == 1)
            {
                return inNumbers[1];
            }
            else
            {
                return inNumbers[2];
            }
        }
    }
}
