//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sym.Nodes
{
    public class Node
    {
        public Node Parent;
        public List<Node> Children = new List<Node>();

        public static string Join(Node inNode, string useNumbers = "")
        {
            string outExpression = JoinNest(inNode, useNumbers);
            return RemoveUneccessaryOperators(outExpression);
        }

        public static string JoinNest(Node inNode, string useNumbers = "")
        {
            if (inNode is OperatorNode)
            {
                List<string> operandStrings = new List<string>();
                OperatorNode operatorNode = (OperatorNode)inNode;
                string operatorString = operatorNode.Operator.OperatorString;
                foreach (Node child in inNode.Children)
                {
                    string operandString = JoinNest(child, useNumbers);
                    if (child is OperatorNode)
                    {
                        OperatorNode childOperatorNode = (OperatorNode)child;
                        if (childOperatorNode.Operator.OperatorType == OperatorType.Delimiting)
                        {
                            string childOperatorString = childOperatorNode.Operator.OperatorString;
                            if (operatorString == "-")
                            {
                                if (operatorNode.Children[0] != childOperatorNode && operatorNode.Operator.OperatorType == OperatorType.Delimiting)
                                {
                                    if (childOperatorString == "+" || childOperatorString == "-")
                                    {
                                        operandString = "(" + operandString + ")";
                                    }
                                }
                                else if (operatorNode.Operator.OperatorType == OperatorType.Unary)
                                {
                                    if (childOperatorString == "+" || childOperatorString == "-" || childOperatorString == "," || childOperatorString == "=")
                                    {
                                        operandString = "(" + operandString + ")";
                                    }
                                }
                            }
                            else if (operatorString == "/")
                            {
                                if (childOperatorString == "+" || childOperatorString == "-" || childOperatorString == "," || childOperatorString == "=")
                                {
                                    operandString = "(" + operandString + ")";
                                }
                                else if (operatorNode.Children[0] != childOperatorNode)
                                {
                                    if (childOperatorString == "*" || childOperatorString == "/")
                                    {
                                        operandString = "(" + operandString + ")";
                                    }
                                }
                            }
                            else if (operatorString == "*")
                            {
                                if (childOperatorString == "+" || childOperatorString == "-" || childOperatorString == "," || childOperatorString == "=")
                                {
                                    operandString = "(" + operandString + ")";
                                }
                            }
                            else if (operatorString == "+")
                            {
                                if (childOperatorString == "," || childOperatorString == "=")
                                {
                                    operandString = "(" + operandString + ")";
                                }
                            }
                        }
                    }
                    operandStrings.Add(operandString);
                }
                if (operatorNode.Operator.OperatorType == OperatorType.Delimiting)
                {
                    string outString = string.Join(operatorNode.Operator.OperatorString, operandStrings.ToArray());
                    return outString;
                }
                else if (operatorNode.Operator.OperatorType == OperatorType.Unary)
                {
                    string outString = operatorNode.Operator.OperatorString + operandStrings[0];
                    return outString;
                }
                else if (operatorNode.Operator.OperatorType == OperatorType.Enclosing)
                {
                    string outString = operatorNode.Operator.OperatorString + "(" + string.Join(",", operandStrings.ToArray()) + ")";
                    return outString;
                }
            }
            else if (inNode is VariableNode)
            {
                VariableNode variableNode = (VariableNode)inNode;
                if (useNumbers != "" && variableNode.Variable.Substring(0, useNumbers.Length) == useNumbers)
                {
                    return variableNode.Number.ToString().Trim();
                }
                return variableNode.Variable;
            }
            else if (inNode is NumericNode)
            {
                NumericNode numericNode = (NumericNode)inNode;
                return numericNode.Number.ToString().Trim();
            }
            return null;
        }

        public static Node Parse(string inExpression, List<Operator> operators)
        {
            //inExpression = RemoveEnclosingBrackets(inExpression);
            double number;
            if (double.TryParse(inExpression, out number))
            {
                NumericNode numericNode = new NumericNode();
                numericNode.Number = number;
                return numericNode;
            }
            List<int> bracketDepths = BracketDepths(inExpression);
            bool foundOperator = false;
            foreach (Operator o in operators)
            {
                List<string> operands = null;
                if (o.OperatorType == OperatorType.Delimiting)
                {
                    operands = ParseDelimiting(inExpression, o, bracketDepths, out foundOperator);
                }
                else if (o.OperatorType == OperatorType.Unary)
                {
                    operands = ParseUnary(inExpression, o, out foundOperator);
                }
                else if (o.OperatorType == OperatorType.Enclosing)
                {
                    operands = ParseEnclosing(inExpression, o, out foundOperator);
                }
                if (foundOperator)
                {
                    OperatorNode operatorNode = new OperatorNode();
                    foreach (string operand in operands)
                    {
                        Node childNode = Parse(operand, operators);
                        childNode.Parent = operatorNode;
                        operatorNode.Children.Add(childNode);
                    }
                    operatorNode.Operator = o;
                    return operatorNode;
                }
            }
            Node newNode = ParseNewEnclosing(inExpression, out foundOperator, operators);
            if (foundOperator)
            {
                return newNode;
            }
            VariableNode variableNode = new VariableNode();
            variableNode.Variable = inExpression;
            if (inExpression.First() == 'C')
            {
                variableNode.VariableNodeType = VariableNodeType.RequireNumber;
            }
            else if (inExpression.First() == 'V')
            {
                variableNode.VariableNodeType = VariableNodeType.RequireVariable;
            }
            else
            {
                variableNode.VariableNodeType = VariableNodeType.NoRequirement;
            }
            return variableNode;
        }

        public static string RemoveUneccessaryOperators(string inExpression)
        {
            string outExpression = inExpression.Replace("--", "+").Replace("+-", "-").Replace("++", "+");
            if (outExpression.First() == '+')
            {
                outExpression = outExpression.Substring(1, outExpression.Length - 1);
            }
            return outExpression;
        }

        public static Node ParseNewEnclosing(string inExpression, out bool foundOperator, List<Operator> operators)
        {
            foundOperator = false;
            int openingBracketLocation = inExpression.IndexOf('(');
            int closingBracketLocation = inExpression.Length - 1;
            if (inExpression.Substring(closingBracketLocation, 1) != ")")
            {
                return null;
            }
            if (openingBracketLocation >= 0)
            {
                foundOperator = true;
                string keyword = inExpression.Substring(0, openingBracketLocation);
                string wholeOperands = inExpression.Substring(openingBracketLocation + 1, inExpression.Length - openingBracketLocation - 2);
                Operator commaOperator = new Operator(",", 0, OperatorType.Delimiting, null);
                List<int> bracketDepths = Node.BracketDepths(wholeOperands);
                bool foundOperator2 = false;
                List<string> operands = Node.ParseDelimiting(wholeOperands, commaOperator, bracketDepths, out foundOperator2);
                OperatorNode newNode = new OperatorNode();
                Operator o = operators.FirstOrDefault(x => x.OperatorString == keyword);
                if (o == null)
                {
                    o = new Operator(keyword, operators.Count, OperatorType.Enclosing, null);
                    operators.Add(o);
                }
                newNode.Operator = o;
                foreach (string operand in operands)
                {
                    Node childNode = Parse(operand, operators);
                    childNode.Parent = newNode;
                    newNode.Children.Add(childNode);
                }
                return newNode;
            }
            return null;
        }

        public static List<string> ParseEnclosing(string inExpression, Operator o, out bool foundOperator)
        {
            foundOperator = false;
            if (inExpression.Length > o.OperatorString.Length + 2 && inExpression.Substring(0, o.OperatorString.Length) == o.OperatorString)
            {
                if (inExpression.Substring(o.OperatorString.Length, 1) == "(" && inExpression.Last() == ')')
                {
                    foundOperator = true;
                    string operandsString = inExpression.Substring(o.OperatorString.Length + 1, inExpression.Length - o.OperatorString.Length - 2);
                    Operator commaOperator = new Operator(",", 0, OperatorType.Delimiting, null);
                    List<int> bracketDepths = Node.BracketDepths(operandsString);
                    bool foundOperator2 = false;
                    List<string> operands = Node.ParseDelimiting(operandsString, commaOperator, bracketDepths, out foundOperator2);
                    return operands;
                }
            }
            return null;
        }

        public static List<string> ParseUnary(string inExpression, Operator o, out bool foundOperator)
        {
            foundOperator = false;
            if (inExpression.Length > o.OperatorString.Length && inExpression.Substring(0, o.OperatorString.Length) == o.OperatorString)
            {
                string operandsString = inExpression.Substring(o.OperatorString.Length, inExpression.Length - o.OperatorString.Length);
                double number;
                if (o.OperatorString == "-" && double.TryParse(operandsString, out number))
                {
                    return null;
                }
                List<string> operands = new List<string>();
                operands.Add(operandsString);
                foundOperator = true;
                return operands;
            }
            return null;
        }

        public static List<string> ParseDelimiting(string inExpression, Operator o, List<int> bracketDepths, out bool foundOperator)
        {
            foundOperator = false;
            List<string> operands = new List<string>();
            int lastStartOperand = 0;
            for (int i = 1; i < inExpression.Length - o.OperatorString.Length; i++)
            {
                if (bracketDepths[i] == 0)
                {
                    string tester = inExpression.Substring(i, o.OperatorString.Length);
                    if (tester == o.OperatorString)
                    {
                        string previous = inExpression.Substring(i - 1, 1);
                        if (!(tester == "-" && (previous == "+" | previous == "-" | previous == "*" | previous == "/")))
                        {
                            foundOperator = true;
                            string operand = inExpression.Substring(lastStartOperand, i - lastStartOperand);
                            operands.Add(operand);
                            lastStartOperand = i + o.OperatorString.Length;
                        }
                    }
                }
            }
            string lastOperand = inExpression.Substring(lastStartOperand, inExpression.Length - lastStartOperand);
            operands.Add(lastOperand);
            return operands;
        }

        //public static string RemoveEnclosingBrackets(string cleanMe)
        //{
        //    if (cleanMe.First() == '(' && cleanMe.Last() == ')')
        //    {
        //        string test = cleanMe.Substring(1, cleanMe.Length - 2);
        //        if (test.Contains("(") == false && test.Contains(")") == false)
        //        {
        //            return test;
        //        }
        //    }
        //    return cleanMe;
        //}

        public static string RemoveEnclosingBrackets(string cleanMe)
        {
            if (cleanMe.First() == '(' && cleanMe.Last() == ')')
            {
                List<int> bracketDepths = BracketDepths(cleanMe);
                if (bracketDepths.Last() == 0)
                {
                    return cleanMe.Substring(1, cleanMe.Length - 2);
                }
            }
            return cleanMe;
        }

        public static List<int> BracketDepths(string inExpression)
        {
            int bracketDepth = 0;
            List<int> bracketDepths = new List<int>();
            for (int i = 0; i < inExpression.Length; i++)
            {
                string character = inExpression.Substring(i, 1);
                if (character == "(")
                {
                    bracketDepth++;
                }
                else if (character == ")")
                {
                    bracketDepth--;
                }
                bracketDepths.Add(bracketDepth);
            }
            return bracketDepths;
        }

        public Node Clone()
        {
            Node newNode = null;
            if (this is OperatorNode)
            {
                OperatorNode oldOperatorNode = (OperatorNode)this;
                newNode = new OperatorNode();
                OperatorNode newOperatorNode = (OperatorNode)newNode;
                newOperatorNode.Operator = oldOperatorNode.Operator;
                foreach (Node child in oldOperatorNode.Children)
                {
                    Node newChild = child.Clone();
                    newChild.Parent = newOperatorNode;
                    newOperatorNode.Children.Add(newChild);
                }
            }
            else if (this is NumericNode)
            {
                NumericNode oldNumericNode = (NumericNode)this;
                newNode = new NumericNode();
                NumericNode newNumericNode = (NumericNode)newNode;
                newNumericNode.Number = oldNumericNode.Number;
            }
            else if (this is VariableNode)
            {
                VariableNode oldVariableNode = (VariableNode)this;
                newNode = new VariableNode();
                VariableNode newVariableNode = (VariableNode)newNode;
                newVariableNode.Variable = oldVariableNode.Variable;
                newVariableNode.VariableNodeType = oldVariableNode.VariableNodeType;
                newVariableNode.Number = oldVariableNode.Number;
            }
            return newNode;
        }

        public List<Node> DescendantsAndSelf()
        {
            List<Node> list = new List<Node>();
            DescendantsAndSelfNest(this, list);
            return list;
        }

        public static void DescendantsAndSelfNest(Node inNode, List<Node> list)
        {
            foreach (Node child in inNode.Children)
            {
                DescendantsAndSelfNest(child, list);
            }
            list.Add(inNode);
        }

        public static Node ReplaceBranches(Node root, List<Node> replaceMes, List<Node> replacements)
        {
            for (int i = 0; i < replaceMes.Count; i++)
            {
                root = ReplaceBranch(root, replaceMes[i], replacements[i]);
            }
            return root;
        }

        public static Node ReplaceBranch(Node replaceMyBranch, Node replaceMe, Node newBranch)
        {
            List<Node> branches = replaceMyBranch.DescendantsAndSelf();
            int branchIndex = branches.IndexOf(replaceMe);
            return ReplaceBranch(branches, branchIndex, newBranch);
        }


        public static Node ReplaceBranch(List<Node> replaceMyBranch, int branchIndex, Node newBranch)
        {
            Node replaceMe = replaceMyBranch[branchIndex];
            if (replaceMe.Parent != null)
            {
                int replaceIndex = replaceMe.Parent.Children.IndexOf(replaceMe);
                replaceMe.Parent.Children.RemoveAt(replaceIndex);
                replaceMe.Parent.Children.Insert(replaceIndex, newBranch);
                newBranch.Parent = replaceMe.Parent;

                return replaceMyBranch.Last();
            }
            else
            {
                return newBranch;
            }
        }

        public static Node RemoveDoubleNegatives(Node inNode)
        {
            Node workNode = inNode;
            if (workNode is OperatorNode)
            {
                OperatorNode operatorNode = (OperatorNode)workNode;
                if (operatorNode.Operator.OperatorString == "-" && operatorNode.Operator.OperatorType == OperatorType.Unary)
                {
                    Node child = operatorNode.Children[0];
                    if (child is OperatorNode)
                    {
                        OperatorNode childOperatorNode = (OperatorNode)child;
                        if (childOperatorNode.Operator.OperatorString == "-" && childOperatorNode.Operator.OperatorType == OperatorType.Unary)
                        {
                            workNode = childOperatorNode.Children[0];
                        }
                    }
                    else if (child is NumericNode)
                    {
                        NumericNode childNumericNode = (NumericNode)child;
                        if (childNumericNode.Number < 0)
                        {
                            childNumericNode.Number *= -1;
                            workNode = childNumericNode;
                        }
                    }
                }
            }
            List<Node> newChildren = new List<Node>();
            foreach (Node child in workNode.Children)
            {
                Node newChild = RemoveDoubleNegatives(child);
                newChild.Parent = workNode;
                newChildren.Add(newChild);
            }
            workNode.Children = newChildren;
            return workNode;
        }

        public static List<Node> ParseSystem(String inSystem, List<Operator> operators)
        {
            string[] rows = inSystem.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return rows.Select(x => Parse(x, operators)).ToList();
        }

        public static List<Pointer> BuildPointers(string keyword, List<Node> branches)
        {
            List<Pointer> pointers = new List<Pointer>();
            int keywordLength = keyword.Length;
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                Node branch = branches[branchIndex];
                if (branch is VariableNode)
                {
                    VariableNode variableBranch = (VariableNode)branch;
                    if (variableBranch.Variable.Length >= keywordLength)
                    {
                        if (variableBranch.Variable.Substring(0, keywordLength) == keyword)
                        {
                            Pointer pointer = new Pointer();
                            pointer.DestinationIndex = branchIndex;
                            string lStr = variableBranch.Variable.Substring(keywordLength + 1, variableBranch.Variable.Length - keywordLength - 2);
                            pointer.SourceIndex = Convert.ToInt32(lStr);
                            pointers.Add(pointer);
                        }
                    }
                }
            }
            return pointers;
        }

        public static void SetNumbers(List<Pointer> pointers, List<double> numbers, List<Node> branches)
        {
            foreach (Pointer replacementPointer in pointers)
            {
                VariableNode branch = (VariableNode)branches[replacementPointer.DestinationIndex];
                branch.Number = numbers[replacementPointer.SourceIndex];
            }
        }

        public static void SetNumbers(List<Pointer> pointers, double[] numbers, List<Node> branches)
        {
            foreach (Pointer replacementPointer in pointers)
            {
                VariableNode branch = (VariableNode)branches[replacementPointer.DestinationIndex];
                branch.Number = numbers[replacementPointer.SourceIndex];
            }
        }
    }
}
