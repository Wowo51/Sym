//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Sym;
using Sym.Nodes;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SymAI.Regression
{
    public class ModelsManager
    {
        public List<Model> Models;
        public List<Transform> Transforms;
        public List<Operator> Operators = Operator.BuildOperators();
        public bool Regressing = false;
        public bool StopRegressing = false;
        public int PopulationSize = 2000;
        public int SurvivingPopulationSize = 250;
        public int BreedPopulationSize = 1000;
        public int MutatePopulationSize = 100;
        public int MaxNodesPerExpression = 15;
        public event Action FinishedGeneration;
        public event Action FinishedRegressing;
        public event Action Calculate;
        public event Action<Individual> PostIndividual;
        public CorrelationItem[] CorrelationItems;
        public List<Node> Independents;
        Random Random = new Random();

        public void Run(int totalIndependents, List<string> startModels, List<Transform> transforms)
        {
            Regressing = true;
            StopRegressing = false;
            Transforms = transforms;
            Independents = BuildIndependents(totalIndependents);
            if (startModels != null && startModels.Count > 0)
            {
                Models = StartExpressionsToModels(startModels, Operators);
            }
            else
            {
                InitializeModels();
            }
            while (StopRegressing == false)
            {
                Breed();
                Mutate();
                Models = Models.Where(x => x.Expression.DescendantsAndSelf().Count <= MaxNodesPerExpression)
                    .GroupBy(x => x.ExpressionString)
                    .Select(g => g.First())
                    .Distinct()
                    .ToList();

                for (int modelIndex = 0; modelIndex < Models.Count; modelIndex++)
                {
                    Models[modelIndex].PostIndividual += Model_PostIndividual;
                }
                foreach (Model model in Models)
                {
                    model.Optimize();
                }
                for (int modelIndex = 0; modelIndex < Models.Count; modelIndex++)
                {
                    Models[modelIndex].PostIndividual -= Model_PostIndividual;
                }

                Calculate();

                Models = Models.OrderByDescending(x => x.LengthAdjustedFitness).ToList();
                //FinishedGeneration();

                int totalUntaken = Models.Count - SurvivingPopulationSize;
                List<Model> unTaken;
                if (totalUntaken < 0)
                {
                    totalUntaken = 0;
                    unTaken = new List<Model>();                    
                }
                else
                {
                    unTaken = Models.GetRange(SurvivingPopulationSize, totalUntaken);
                }
                List<Model> unOptimized = unTaken.Where(x => x.Optimizer.IsOptimized() == false).ToList();

                Models = Models.Take(SurvivingPopulationSize).ToList();
                Models.AddRange(unOptimized);
                Models = Models.Distinct().ToList();

               
                Models = Models.Where(x => x.LengthAdjustedFitness > double.MinValue).ToList();
                FinishedGeneration();
            }
            Regressing = false;
            FinishedRegressing();
        }

        class ModelComparer : IEqualityComparer<Model>
        {
            public bool Equals(Model x, Model y)
            {
                return x.ExpressionString == y.ExpressionString;
            }

            public int GetHashCode(Model model)
            {
                return  model.ExpressionString.GetHashCode();
            }
        }

        public void InitializeModels()
        {
            Models = new List<Model>();
            while (Models.Count < SurvivingPopulationSize)
            {
                Model model = CreateTransformedModel();
                EliminateFs(model, Independents);
                if (model.Expression.DescendantsAndSelf().Count <= MaxNodesPerExpression)
                {
                    Models.Add(model);
                }
            }
        }

        private void Model_PostIndividual(Individual individual)
        {
            PostIndividual(individual);
        }

        public List<Node> BuildIndependents(int totalIndependents)
        {
            List<Node> independents = new List<Node>();
            for (int i = 0; i < totalIndependents; i++)
            {
                string independent = "Independent[" + i.ToString().Trim() + "]";
                independents.Add(Node.Parse(independent, Operators));
            }
            return independents;
        }

        public Model CreateTransformedModel()
        {
            if (Models.Count == 0)
            {
                Model model = new Model();
                model.Expression = Node.Parse("f", Operators);
                model.ExpressionString = "f"; 
                //model.TotalConstants = Regex.Matches(model.ExpressionString, "Constant").Count;
                return model;
            }
            else
            {
                int modelIndex = RandomHelpers.ExponentialSelector(Math.Min(SurvivingPopulationSize, Models.Count), 2d);
                Model cloneModel = Models[modelIndex].Clone();
                Node startExpression = cloneModel.Expression;
                int transformIndex = Random.Next(0, Transforms.Count);
                Transform transform = Transforms[transformIndex];
                //cloneModel.Expression = TransformBranchFunctions.TransformRandomBranch(startExpression, transform, Operators);
                cloneModel.Expression = TransformRandomBranchExponentially(startExpression, transform, Operators);
                cloneModel.ExpressionString = Node.Join(cloneModel.Expression);
                //cloneModel.TotalConstants = Regex.Matches(cloneModel.ExpressionString, "Constant").Count;
                return cloneModel;
            }
        }

        public static Node TransformRandomBranchExponentially(Node root, Transform transform, List<Operator> operators)
        {
            List<Node> branches = root.DescendantsAndSelf();
            int branchIndex = branches.Count - 1 - RandomHelpers.ExponentialSelector(branches.Count, 1);
            return TransformBranchFunctions.TransformBranchWithTransform(root, transform, operators, branchIndex);
        }

        public List<Model> StartExpressionsToModels(List<string> startModels, List<Operator> operators)
        {
            List<Model> newModels = new List<Model>();
            foreach (string expression in startModels)
            {
                Model model = new Model();
                model.ExpressionString = expression;
                model.Expression = Node.Parse(expression, operators);
                //model.TotalConstants = Regex.Matches(expression, "Constant").Count;
                //model.BuildConstantPointers();
                newModels.Add(model);
            }
            return newModels;
        }

        public void EliminateFs(Model model, List<Node> independents)
        {
            bool containsFs = true;
            while (containsFs)
            {
                List<Node> branches = model.Expression.DescendantsAndSelf();
                List<Node> fBranches = branches.Where(x => x.Children.Count == 0 && Node.Join(x) == "f").ToList();
                if (fBranches.Count == 0)
                {
                    containsFs = false;
                    break;
                }
                List<Node> replacementBranches = new List<Node>();
                foreach (Node fBranch in fBranches)
                {
                    int replacementType = Random.Next(0, 3);
                    if (replacementType == 0)
                    {
                        replacementBranches.Add(Node.Parse("Constant", Operators));
                    }
                    else if (replacementType == 1)
                    {
                        int independentIndex = RandomHelpers.ExponentialSelector(independents.Count, 2d);
                        Node independentNode = independents[CorrelationItems[independentIndex].ColumnIndex].Clone();
                        replacementBranches.Add(independentNode);
                    }
                    else if (replacementType == 2)
                    {
                        int transformIndex = Random.Next(0, Transforms.Count);
                        Transform transform = Transforms[transformIndex];
                        Node transformedNode = Transform.TransformNode(fBranch, transform, Operators);
                        replacementBranches.Add(transformedNode);
                    }
                }
                model.Expression = Node.ReplaceBranches(model.Expression, fBranches, replacementBranches);

            }
            ResetConstants(model);
            
        }

        public void ResetConstants(Model model)
        {
            List<Node> constantBranches = GetConstantBranches(model.Expression);
            int totalConstantsRequired = constantBranches.Count;
            List<Node> constants = new List<Node>();
            for (int constantIndex = 0; constantIndex < totalConstantsRequired; constantIndex++)
            {
                constants.Add(Node.Parse("Constant[" + constantIndex.ToString().Trim() + "]", Operators));
            }
            Node result = Node.ReplaceBranches(model.Expression, constantBranches, constants);
            model.Expression = result;
            model.ExpressionString = Node.Join(model.Expression);
            //model.TotalConstants = constants.Count;
            //model.BuildConstantPointers();
        }

        public List<Node> GetConstantBranches(Node expression)
        {
            List<Node> constantBranches = new List<Node>();
            foreach (Node branch in expression.DescendantsAndSelf())
            {
                if (branch.Children.Count == 0)
                {
                    string lStr = Node.Join(branch);
                    if (lStr.Length >= 8)
                    {
                        if (lStr.Substring(0, 8) == "Constant")
                        {
                            constantBranches.Add(branch);
                        }
                    }
                }
            }
            return constantBranches;
        }

        public bool ModelExists(Model model)
        {
            return Models.AsParallel().Any(x => x.ExpressionString == model.ExpressionString);
        }

        public void Breed()
        {
            ConcurrentBag<Model> children = new ConcurrentBag<Model>();
            int totalModelsToCreate = Math.Min(PopulationSize - Models.Count, BreedPopulationSize);
            Parallel.For(0, totalModelsToCreate, bagIndex =>
            {
                Model child = new Model();
                int parent1Index = RandomHelpers.ExponentialSelector(Math.Min(SurvivingPopulationSize, Models.Count), 2d);
                int parent2Index = RandomHelpers.ExponentialSelector(Math.Min(SurvivingPopulationSize, Models.Count), 2d);
                if (parent1Index != parent2Index)
                {
                    Node parent1Expression = Models[parent1Index].Expression;
                    Node parent2Expression = Models[parent2Index].Expression.Clone();
                    List<Node> parent1Branches = parent1Expression.DescendantsAndSelf();
                    List<Node> parent2Branches = parent2Expression.DescendantsAndSelf();
                    int branchToMoveIndex = Random.Next(parent1Branches.Count);
                    int branchLocationToInjectAt = Random.Next(parent2Branches.Count);
                    Node branchToMove = parent1Branches[branchToMoveIndex].Clone();
                    Node branchToReplace = parent2Branches[branchLocationToInjectAt];
                    if (branchToReplace.Parent != null)
                    {
                        int childIndex = branchToReplace.Parent.Children.IndexOf(branchToReplace);
                        branchToReplace.Parent.Children.RemoveAt(childIndex);
                        branchToReplace.Parent.Children.Insert(childIndex, branchToMove);
                        branchToMove.Parent = branchToReplace.Parent;
                        child.Expression = parent2Expression;
                        child.ExpressionString = Node.Join(child.Expression);
                        ResetConstants(child);
                        //child.TotalConstants = Regex.Matches(child.ExpressionString, "Constant").Count;
                        children.Add(child);
                    }
                }
            });
            Models.AddRange(children.ToList());
        }

        public void Mutate()
        {
            ConcurrentBag<Model> mutants = new ConcurrentBag<Model>();
            int totalModelsToCreate = Math.Min(PopulationSize - Models.Count, BreedPopulationSize);
            Parallel.For(0, totalModelsToCreate, bagIndex =>
            {
                Model model = CreateTransformedModel();
                EliminateFs(model, Independents);
                if (model.Expression.DescendantsAndSelf().Count <= MaxNodesPerExpression)
                {
                    mutants.Add(model);
                }
            });
            Models.AddRange(mutants.ToList());
        }

        public List<Transform> GenerateTransforms()
        {
            List<string> lOut = new List<string>();
            lOut.Add("x~f+f");
            lOut.Add("x~f-f");
            lOut.Add("x~f*f");
            lOut.Add("x~f/f");
            //lOut.Add("x~Sin(f)");
            //lOut.Add("x~Cos(f)");
            //lOut.Add("x~Tan(f)");
            //lOut.Add("x~Pow(f,f)");
            //lOut.Add("x~Sign(f)");
            //lOut.Add("x~Abs(f)");
            lOut.Add("x~f");
            return lOut.Select(x => Transform.StringToTransform(x, Operators)).ToList();
        }
    }
}
