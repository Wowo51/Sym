//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;
using System.Linq;

namespace Sym
{
    public class Transform
    {
        public Node Left;
        public Node Right;

        public class ReplacementPair
        {
            public string ReplaceMe;
            public string Replacement;
            public Node ReplacementNode;
        }

        public static Node TransformNode(Node transformMe, Transform transform, List<Operator> operators)
        {
            bool didMatch = MatchWalk(transformMe, transform.Left);
            if (didMatch == true)
            {
                Node transformed = TransformMatchedNode(transformMe, transform, operators);
                if (transformed != null)
                {
                    Node cleaned = Node.RemoveDoubleNegatives(transformed);
                    return cleaned;
                }
            }
            return null;
        }

        public static bool MatchWalk(Node transformMe, Node matchToMe)
        {
            bool didMatch = Match(transformMe, matchToMe);
            if (didMatch == false)
            {
                return false;
            }
            if (matchToMe.Children.Count > 0)
            {
                List<Node> matchToMeChildren = matchToMe.Children;
                List<Node> transformMeChildren = transformMe.Children;
                if (matchToMeChildren.Count > 0)
                {
                    if (matchToMeChildren.Count != transformMeChildren.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < transformMeChildren.Count; i++)
                    {

                        bool lBool = MatchWalk(transformMeChildren[i], matchToMeChildren[i]);
                        if (lBool == false)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool Match(Node transformMe, Node matchToMe)
        {
            if (matchToMe is OperatorNode && transformMe is OperatorNode)
            {
                OperatorNode matchToMeOperator = (OperatorNode)matchToMe;
                OperatorNode transformMeOperator = (OperatorNode)transformMe;
                if (matchToMeOperator.Operator.OperatorIndex == transformMeOperator.Operator.OperatorIndex)
                {
                    if (matchToMeOperator.Children.Count == transformMeOperator.Children.Count)
                    {
                        return true;
                    }
                }
            }
            else if (matchToMe is NumericNode && transformMe is NumericNode)
            {
                NumericNode matchToMeNumeric = (NumericNode)matchToMe;
                NumericNode transformMeNumeric = (NumericNode)transformMe;
                if (matchToMeNumeric.Number == transformMeNumeric.Number)
                {
                    return true;
                }
            }
            else if (matchToMe is VariableNode)
            {
                VariableNode matchToMeVariable = (VariableNode)matchToMe;
                if (matchToMeVariable.VariableNodeType == VariableNodeType.RequireNumber)
                {
                    if (transformMe is NumericNode)
                    {
                        return true;
                    }
                }
                else if (matchToMeVariable.VariableNodeType == VariableNodeType.RequireVariable)
                {
                    if (transformMe is VariableNode)
                    {
                        if (matchToMeVariable.Variable.Length == 1)
                        {
                            return true;
                        }
                        else
                        {
                            string match = matchToMeVariable.Variable.Substring(1, matchToMeVariable.Variable.Length - 1);
                            VariableNode transformMeVariable = (VariableNode)transformMe;
                            if (match == transformMeVariable.Variable)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (matchToMeVariable.VariableNodeType == VariableNodeType.NoRequirement)
                {
                    return true;
                }
            }
            return false;
        }

        public static Node TransformMatchedNode(Node transformMe, Transform transform, List<Operator> operators)
        {
            List<ReplacementPair> replacementPairs = new List<ReplacementPair>();
            bool foundMisMatch = false;
            GetReplacementPairs(transform.Left.Clone(), transformMe, replacementPairs, ref foundMisMatch, operators);
            if (foundMisMatch == false)
            {
                return ReplaceFunctions(transform.Right.Clone(), replacementPairs, operators);
            }
            return null;
        }

        public static void GetReplacementPairs(Node matchToMe, Node transformMe, List<ReplacementPair> replacementPairs, ref bool foundMisMatch, List<Operator> operators)
        {
            if (foundMisMatch == true)
            {
                return;
            }
            string matchToMeString = Node.Join(matchToMe);
            string transformMeString = Node.Join(transformMe);
            if (matchToMe is VariableNode)
            {
                bool ReplacementPairExists = false;
                for (int L = 0; L < replacementPairs.Count; L++)
                {
                    if (replacementPairs[L].ReplaceMe == matchToMeString)
                    {
                        ReplacementPairExists = true;
                        if (replacementPairs[L].Replacement != transformMeString)
                        {
                            foundMisMatch = true;
                            break;
                        }
                    }
                }
                if (ReplacementPairExists == false)
                {
                    ReplacementPair lPair = new ReplacementPair();
                    lPair.ReplaceMe = matchToMeString;
                    lPair.Replacement = transformMeString;
                    lPair.ReplacementNode = transformMe;
                    replacementPairs.Add(lPair);
                }

            }
            if (foundMisMatch == false)
            {
                if (matchToMe.Children.Count > 0)
                {
                    List<Node> matchToMeChildren = matchToMe.Children;
                    List<Node> transformMeChildren = transformMe.Children;
                    if (transformMeChildren.Count >= matchToMeChildren.Count)
                    {
                        for (int i = 0; i < matchToMeChildren.Count; i++)
                        {
                            GetReplacementPairs(matchToMeChildren[i], transformMeChildren[i], replacementPairs, ref foundMisMatch, operators);
                            if (foundMisMatch == true)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        foundMisMatch = true;
                        return;
                    }
                }
            }
        }

        public static Node ReplaceFunctions(Node inTree, List<ReplacementPair> replacementPairs, List<Operator> operators)
        {
            //Node tree = inTree.Clone();
            if (inTree is VariableNode)
            {
                VariableNode node = (VariableNode)inTree;
                for (int replacementPairIndex = 0; replacementPairIndex < replacementPairs.Count; replacementPairIndex++)
                {
                    if (node.Variable == replacementPairs[replacementPairIndex].ReplaceMe)
                    {
                        return replacementPairs[replacementPairIndex].ReplacementNode.Clone();
                    }
                }
            }
            List<Node> newChildren = new List<Node>();
            foreach (Node child in inTree.Children)
            {
                Node newChild = ReplaceFunctions(child, replacementPairs, operators);
                newChild.Parent = inTree;
                newChildren.Add(newChild);
            }
            inTree.Children = newChildren;
            return inTree;
        }

        public static Transform StringToTransform(string inTransform, List<Operator> operators)
        {
            string[] transformStrings = inTransform.Split('~');
            if (transformStrings.Any(x => x.Length == 0))
            {
                return null;
            }
            if (transformStrings.Length != 2)
            {
                return null;
            }
            List<Node> transformTrees = new List<Node>();
            foreach (string transformString in transformStrings)
            {
                Node transformTree = Node.Parse(transformString, operators);
                transformTrees.Add(transformTree);
            }
            Transform transform = new Transform();
            transform.Left = transformTrees[0];
            transform.Right = transformTrees[1];
            return transform;
        }

        public static string TransformToString(Transform transform)
        {
            return Node.Join(transform.Left) + "~" + Node.Join(transform.Right);
        }
    }
}
