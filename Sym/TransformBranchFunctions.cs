//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sym.Nodes;

namespace Sym
{
    public class TransformBranchFunctions
    {
        static Random Random = new Random();

        public static List<Node> TransformBranchesWithTransforms(Node root, List<Transform> transforms, List<Operator> operators)
        {
            List<Node> lOut = new List<Node>();
            for (int transformIndex = 0; transformIndex < transforms.Count; transformIndex++)
            {
                List<Node> tempNodes = TransformBranchesWithTransform(root, transforms[transformIndex], operators);
                lOut.AddRange(tempNodes);
            }
            return lOut;
        }

        public static List<Node> TransformBranchesWithTransform(Node root, Transform transform, List<Operator> operators)
        {
            Node rootClone1 = root.Clone();
            List<Node> lOut = new List<Node>();
            List<Node> branches1 = rootClone1.DescendantsAndSelf();
            for (int branchIndex = 0; branchIndex < branches1.Count; branchIndex++)
            {
                Node branch = branches1[branchIndex];
                Node transformedBranch = Transform.TransformNode(branch, transform, operators);
                if (transformedBranch != null)
                {
                    Node rootClone2 = root.Clone();
                    List<Node> branches2 = rootClone2.DescendantsAndSelf();
                    Node newRoot = Node.ReplaceBranch(branches2, branchIndex, transformedBranch);
                    lOut.Add(newRoot);
                }
            }
            return lOut;
        }

        public static Node TransformBranchesWithTransformsToOneResult(Node root, List<Transform> transforms, List<Operator> operators)
        {
            Node workNode = root.Clone();
            bool didTransform = false;
            for (int transformIndex = 0; transformIndex < transforms.Count; transformIndex++)
            {
                workNode = TransformBranchesWithTransformToOneResult(workNode, transforms[transformIndex], operators, ref didTransform);
            }
            if (didTransform)
            {
                return workNode;
            }
            return null;
        }

        public static Node TransformBranchesWithTransformToOneResult(Node node, Transform inTransform, List<Operator> operators, ref bool didTransform)
        {
            return TransformBranchesWithTransformToOneResultNest(node.Clone(), inTransform, operators, ref didTransform);
        }

        public static Node TransformBranchesWithTransformToOneResultNest(Node node, Transform inTransform, List<Operator> operators, ref bool didTransform)
        {
            Node testNode = Transform.TransformNode(node, inTransform, operators);
            if (testNode != null)
            {
                didTransform = true;
                return testNode;
            }
            List<Node> newChildren = new List<Node>();
            foreach (Node child in node.Children)
            {
                Node newChild = TransformBranchesWithTransformToOneResultNest(child, inTransform, operators, ref didTransform);
                newChild.Parent = node;
                newChildren.Add(newChild);
            }
            node.Children = newChildren;
            return node;
        }

        public static Node TransformRandomBranch(Node root, Transform transform, List<Operator> operators)
        {
            List<Node> branches = root.DescendantsAndSelf();
            int branchIndex = Random.Next(0, branches.Count);
            return TransformBranchWithTransform(root, transform, operators, branchIndex);
        }

        public static Node TransformBranchWithTransform(Node root, Transform transform, List<Operator> operators, int branchIndex)
        {
            Node rootClone = root.Clone();
            List<Node> lOut = new List<Node>();
            List<Node> branches = rootClone.DescendantsAndSelf();
            Node branch = branches[branchIndex];
            Node transformedBranch = Transform.TransformNode(branch, transform, operators);
            if (transformedBranch != null)
            {
                Node newRoot = Node.ReplaceBranch(branches, branchIndex, transformedBranch);
                return newRoot;
            }
            return null;
        }
    }
}
