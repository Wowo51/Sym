using System;
using System.Collections.Generic;
using System.Text;
using Sym.Nodes;
using Sym.Goals;

namespace Sym
{
    public class Solvers : Solver
    {
        public string Simplify(string simplifyMe, int maxRepetitions, int maxPopulationSize)
        {
            List<Operator> operators = Operator.BuildOperators();
            return Simplify(simplifyMe, maxPopulationSize, maxPopulationSize, operators);
        }

        public string Simplify(string simplifyMe, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Goal> goals = TransformGoal.StandardFormTransformGoals(operators);
            goals.Add(new LengthGoal());
            goals.Add(new PreferOneVariableGoal());
            List<Transform> transforms = TransformData.AllAlgebraTransformsAsTransforms(operators);
            UseSubstitutions = false;
            return Solve(simplifyMe, goals, transforms, maxRepetitions, maxPopulationSize, operators);
        }

        public string SolveSystem(string transformMe, string isolateMe, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Goal> goals = TransformGoal.StandardFormTransformGoals(operators);
            goals.Add(new IsolateGoal(isolateMe, true));
            goals.Add(new LengthGoal());
            List<Transform> transforms = TransformData.AllAlgebraTransformsAsTransforms(operators);
            UseSubstitutions = true;
            return Solve(transformMe, goals, transforms, maxRepetitions, maxPopulationSize, operators);
        }

        public string Isolate(string transformMe, string isolateMe, int maxRepetitions, int maxPopulationSize)
        {
            List<Operator> operators = Operator.BuildOperators();
            return Isolate(transformMe, isolateMe, maxRepetitions, maxPopulationSize, operators);
        }

        public string Isolate(string transformMe, string isolateMe, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Goal> goals = TransformGoal.StandardFormTransformGoals(operators);
            goals.Add(new IsolateGoal(isolateMe, false));
            goals.Add(new LengthGoal());
            List<Transform> transforms = TransformData.AllAlgebraTransformsAsTransforms(operators);
            UseSubstitutions = false;
            return Solve(transformMe, goals, transforms, maxRepetitions, maxPopulationSize, operators);
        }

        public string Derivative(string transformMe, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Goal> goals = TransformGoal.StandardFormTransformGoals(operators);
            goals.Add(new TransformGoal(-50, "d(x)~0", operators));
            List<Transform> transforms = TransformData.AllAlgebraTransformsAsTransforms(operators);
            transforms.AddRange(TransformData.DerivativeTransformsAsTransforms(operators));
            UseSubstitutions = false;
            return Solve(transformMe, goals, transforms, maxRepetitions, maxPopulationSize, operators);
        }

        public string PartialDerivative(string transformMe, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Goal> goals = TransformGoal.StandardFormTransformGoals(operators);
            goals.Add(new TransformGoal(-50, "p(x)~0", operators));
            List<Transform> transforms = TransformData.AllAlgebraTransformsAsTransforms(operators);
            transforms.AddRange(TransformData.PartialDerivativeTransformsAsTransforms(operators));
            UseSubstitutions = false;
            return Solve(transformMe, goals, transforms, maxRepetitions, maxPopulationSize, operators);
        }

        public string Solve(string solveMe, List<Goal> goals, List<Transform> transforms, int maxRepetitions, int maxPopulationSize, List<Operator> operators)
        {
            List<Node> transformMeNodes;
            try
            {
                transformMeNodes = Node.ParseSystem(solveMe, operators);
            }
            catch
            {
                return null;
            }
            Node node = Solve(transformMeNodes, transforms, maxRepetitions, maxPopulationSize, goals, operators);
            return Node.Join(node);
        }
    }
}
