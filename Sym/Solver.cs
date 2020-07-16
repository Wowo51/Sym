using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sym.Goals;
using System.Threading;
using System.Threading.Tasks;
using Sym.Nodes;

namespace Sym
{
    public class Solver
    {
        public bool UseParallel = false;
        public event Action<List<PotentialAnswer>> FinishedRepetition;
        public bool UseSubstitutions = false;
        bool stopSolving = false;
        CancellationTokenSource cancellationTokenSource;

        public Node Solve(List<Node> system, List<Transform> inTransforms, int maxRepetitions, int populationSize, List<Goal> goals, List<Operator> operators)
        {
            stopSolving = false;
            List<Node> system2 = system.ToList();
            for (int i1 = 0; i1 < system.Count; i1++)
            {
                for (int i2 = i1 + 1; i2 < system.Count; i2++)
                {
                    system2.AddRange(Substitute(system[i1], system[i2], operators));
                }
            }
            List<PotentialAnswer> potentialAnswers = new List<PotentialAnswer>();
            foreach (Node solveMe in system2)
            {
                PotentialAnswer solveMeAnswer = new PotentialAnswer();
                Node reducedSolveMe = EvaluateBranches.Evaluate(solveMe);
                if (reducedSolveMe.DescendantsAndSelf().Count < solveMe.DescendantsAndSelf().Count)
                {
                    solveMeAnswer.Equation = reducedSolveMe;
                    solveMeAnswer.EquationString = Node.Join(reducedSolveMe);
                    //solveMeAnswer.Equation = Node.Parse(solveMeAnswer.EquationString, operators);
                }
                else
                {
                    solveMeAnswer.Equation = solveMe;
                    solveMeAnswer.EquationString = Node.Join(solveMe);
                    //solveMeAnswer.Equation = Node.Parse(solveMeAnswer.EquationString, operators);
                }
                solveMeAnswer.Fitness = Goal.CalculateGoalFitness(solveMeAnswer.Equation, goals);
                solveMeAnswer.Isolated = IsIsolated(solveMeAnswer.Equation);
                potentialAnswers.Add(solveMeAnswer);
                
                //PotentialAnswer reparsed = new PotentialAnswer();
                //reparsed.Equation = Node.Parse(solveMeAnswer.EquationString, operators);
                //reparsed.EquationString = solveMeAnswer.EquationString;
                //solveMeAnswer.Fitness = Goal.CalculateGoalFitness(reparsed.Equation, goals);
                //solveMeAnswer.Isolated = IsIsolated(reparsed.Equation);
                //potentialAnswers.Add(reparsed);
            }
            bool foundNonExistantAnswer = false;
            List<double> bestFitnesses = new List<double>();
            int totalBestFitnesses = 5;
            List<string> existingEquationStrings;
            for (int i = 0; i < maxRepetitions; i++)
            {
                List<PotentialAnswer> unTransformed = potentialAnswers.Where(x => x.Transformed == false).ToList();
                foundNonExistantAnswer = false;
                existingEquationStrings = potentialAnswers.Select(x => x.EquationString).ToList();
                if (UseParallel)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    ParallelOptions po = new ParallelOptions();
                    po.CancellationToken = cancellationTokenSource.Token;
                    po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                    try
                    {
                        Parallel.ForEach(unTransformed, po, x => ProcessUntransformedAnswer(x, existingEquationStrings, ref foundNonExistantAnswer, potentialAnswers, operators, goals, inTransforms, ref stopSolving, UseSubstitutions));
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        cancellationTokenSource.Dispose();
                    }

                }
                else
                {
                    foreach (PotentialAnswer potentialAnswer in unTransformed)
                    {
                        ProcessUntransformedAnswer(potentialAnswer, existingEquationStrings, ref foundNonExistantAnswer, potentialAnswers, operators, goals, inTransforms, ref stopSolving, UseSubstitutions);
                        if (stopSolving)
                        {
                            return potentialAnswers.OrderByDescending(x => x.Fitness).First().Equation;
                        }
                    }
                }
                if (foundNonExistantAnswer == false)
                {
                    break;
                }
                potentialAnswers = potentialAnswers.GroupBy(o => o.EquationString).Select(g => g.First()).OrderByDescending(x => x.Fitness).Take(populationSize).ToList();
                bestFitnesses.Add(potentialAnswers[0].Fitness);
                if (bestFitnesses.Count > totalBestFitnesses)
                {
                    bestFitnesses.RemoveAt(0);
                    double firstFitness = bestFitnesses.First();
                    if (bestFitnesses.All(x => x == firstFitness))
                    {
                        break;
                    }
                }
                FinishedRepetition(potentialAnswers);
            }
            return potentialAnswers.First().Equation;
        }

        public static void ProcessUntransformedAnswer(PotentialAnswer potentialAnswer, List<string> existingEquationStrings, ref bool foundNonExistantAnswer, List<PotentialAnswer> potentialAnswers, List<Operator> operators, List<Goal> goals, List<Transform> inTransforms, ref bool stopSolving, bool useSubstitutions)
        {
            List<Node> newAnswers = TransformBranchFunctions.TransformBranchesWithTransforms(potentialAnswer.Equation, inTransforms, operators);
            potentialAnswer.Transformed = true;
            foreach (Node newAnswer1 in newAnswers)
            {
                Node newAnswer2 = EvaluateBranches.Evaluate(newAnswer1);
                ProcessNewAnswer(newAnswer1, existingEquationStrings, ref foundNonExistantAnswer, potentialAnswers, operators, goals);
                ProcessNewAnswer(newAnswer2, existingEquationStrings, ref foundNonExistantAnswer, potentialAnswers, operators, goals);
                if (stopSolving)
                {
                    break;
                }
            }
            if (useSubstitutions)
            {
                Substitute(potentialAnswer.Equation, potentialAnswers, existingEquationStrings, ref foundNonExistantAnswer, operators, goals, ref stopSolving);
            }
        }

        public static void ProcessNewAnswer(Node newAnswer, List<string> existingEquationStrings, ref bool foundNonExistantAnswer,List<PotentialAnswer> potentialAnswers, List<Operator> operators, List<Goal> goals)
        {
            string newEquationString1 = Node.Join(newAnswer);
            if (existingEquationStrings.Contains(newEquationString1) == false)
            {
                foundNonExistantAnswer = true;
                PotentialAnswer newPotential = new PotentialAnswer();
                //newPotential.Equation = newAnswer;
                newPotential.EquationString = newEquationString1;
                newPotential.Equation = Node.Parse(newPotential.EquationString, operators);
                if (newEquationString1.Contains("∞") == false && newEquationString1.Contains("NaN") == false)
                {
                    newPotential.Fitness = Goal.CalculateGoalFitness(newAnswer, goals);
                    newPotential.Isolated = IsIsolated(newPotential.Equation);
                    potentialAnswers.Add(newPotential);
                }
            }
        }

        public class PotentialAnswer
        {
            public Node Equation;
            public string EquationString;
            public double Fitness;
            public bool Transformed = false;
            public bool Isolated = false;
        }

        public bool StopSolving
        {
            get
            {
                return stopSolving;
            }
            set
            {
                if (UseParallel)
                {
                    try
                    {
                        cancellationTokenSource.Cancel();
                    }
                    catch
                    {
                    }
                }
                stopSolving = value;
            }
        }

        public static List<Node> Substitute(Node node1, Node node2, List<Operator> operators)
        {
            List<Node> outNodes = new List<Node>();
            if (IsIsolated(node1))
            {
                Node newNode = SubstituteNest(node2, node1, operators);
                if (newNode != null)
                {
                    outNodes.Add(newNode);
                }
            }
            if (IsIsolated(node2))
            {
                Node newNode = SubstituteNest(node1, node2, operators);
                if (newNode != null)
                {
                    outNodes.Add(newNode);
                }
            }
            return outNodes;
        }

        public static void Substitute(Node newAnswer, List<PotentialAnswer> existingAnswers, List<string> existingEquationStrings, ref bool foundNonExistantAnswer, List<Operator> operators, List<Goal> goals, ref bool stopSolving)
        {
            List<PotentialAnswer> existingIsolated = existingAnswers.Where(x => x.Isolated).ToList();
            foreach (PotentialAnswer potentialAnswer in existingIsolated)
            {
                Node substitution = SubstituteNest(newAnswer, potentialAnswer.Equation, operators);
                if (substitution != null)
                {
                    ProcessNewAnswer(substitution, existingEquationStrings, ref foundNonExistantAnswer, existingAnswers, operators, goals);
                    if (stopSolving)
                    {
                        return;
                    }
                }
            }
            if (IsIsolated(newAnswer))
            {
                foreach (PotentialAnswer potentialAnswer in existingAnswers.ToList())
                {
                    Node substitution = SubstituteNest(potentialAnswer.Equation, newAnswer, operators);
                    if (substitution != null)
                    {
                        ProcessNewAnswer(substitution, existingEquationStrings, ref foundNonExistantAnswer, existingAnswers, operators, goals);
                        if (stopSolving)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public static bool IsIsolated(Node node)
        {
            if (node is OperatorNode)
            {
                OperatorNode nodeOperator = (OperatorNode)node;
                if (nodeOperator.Operator.OperatorString == "=")
                {
                    Node left = nodeOperator.Children[0];
                    if (left is VariableNode)
                    {
                        Node right = nodeOperator.Children[1];
                        List<VariableNode> rightVariables = right.DescendantsAndSelf().Where(x => x is VariableNode).Select(x => (VariableNode)x).ToList();
                        VariableNode leftVariable = (VariableNode)left;
                        if (rightVariables.Any(x => x.Variable == leftVariable.Variable) == false)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static Node SubstituteNest(Node receiver, Node provider, List<Operator> operators)
        {
            VariableNode leftVariable = (VariableNode)provider.Children[0];
            Transform transform = new Transform();
            VariableNode leftTransform = new VariableNode();
            leftTransform.VariableNodeType = VariableNodeType.RequireVariable;
            leftTransform.Variable = "V" + leftVariable.Variable;
            transform.Left = leftTransform;
            transform.Right = provider.Children[1];
            bool didTransform = false;
            Node outNode = TransformBranchFunctions.TransformBranchesWithTransformToOneResult(receiver, transform, operators, ref didTransform);
            if (didTransform)
            {
                return outNode;
            }
            return null;
        }
}
}
