//Copyright Warren Harding 2025.
using System.Collections.Immutable;
using Sym.Core;
using Sym.Core.Rewriters;
using Sym.Atoms;
using Sym.Operations;
using System.Linq;

namespace Sym.Core.Strategies
{
    /// <summary>
    /// **`EquationSolverStrategy`**:
    /// *   **Purpose:** To transform an `Equality` expression into the form `TargetVariable = solution`.
    /// *   **Implementation:** It will validate inputs, repeatedly apply general simplification rules, and then use **isolation tactics**. These tactics involve identifying the outermost operation on the side with the `TargetVariable` and applying inverse operations (e.g., subtracting a term from both sides, dividing by a coefficient). It will check for the goal after each transformation.
    /// </summary>
    public sealed class EquationSolverStrategy : ISolverStrategy
    {
        /// <summary>
        /// Solves a given problem expression (assumed to be an Equality) for a target variable.
        /// </summary>
        /// <param name="problem">The Equality expression to solve (e.g., SomeExpression = OtherExpression).</param>
        /// <param name="context">The context containing solver settings like rules, target variable, and tracing options.</param>
        /// <returns>A SolveResult indicating success/failure, the result equation, and a message.</returns>
        public SolveResult Solve(IExpression? problem, SolveContext context)
        {
            // If problem is null, problem is not Equality will be true.
            if (problem is not Equality currentEquation)
            {
                return SolveResult.Failure(problem, "EquationSolverStrategy requires an Equality expression as input.");
            }

            if (context.TargetVariable is null)
            {
                return SolveResult.Failure(problem, "Target variable must be specified in the SolveContext for EquationSolverStrategy.");
            }
            Symbol targetVariable = context.TargetVariable;

            currentEquation = (Equality)currentEquation.Canonicalize(); // Canonicalize the initial problem
            ImmutableList<IExpression>.Builder? traceBuilder = context.EnableTracing ? ImmutableList.CreateBuilder<IExpression>() : null;
            traceBuilder?.Add(currentEquation);

            bool changedInIteration;

            for (int i = 0; i < context.MaxIterations; i++)
            {
                changedInIteration = false;
                Equality previousEquation = currentEquation;

                // Step 1: Apply full simplification rules
                RewriterResult simplificationResult = Rewriter.RewriteFully(currentEquation, context.Rules);
                if (simplificationResult.Changed)
                {
                    currentEquation = (Equality)simplificationResult.RewrittenExpression;
                    changedInIteration = true;
                    traceBuilder?.Add(currentEquation);
                }
                
                // Step 2: Check if goal is achieved after simplification
                if (CheckGoal(currentEquation, targetVariable))
                {
                    return SolveResult.Success(currentEquation, "Equation solved successfully.", traceBuilder?.ToImmutable());
                }

                // Step 3: Apply isolation tactics
                bool isolationOccurred = false;

                if (currentEquation.LeftOperand.ContainsSymbol(targetVariable) && !currentEquation.RightOperand.ContainsSymbol(targetVariable))
                {
                    // Target variable is on the left side, try to isolate
                    Equality newEquation = IsolateSide(currentEquation.LeftOperand, currentEquation.RightOperand, targetVariable, out isolationOccurred);
                    if (isolationOccurred)
                    {
                        currentEquation = newEquation; // This newEquation is already canonicalized within IsolateSide
                        changedInIteration = true;
                    }
                }
                else if (!currentEquation.LeftOperand.ContainsSymbol(targetVariable) && currentEquation.RightOperand.ContainsSymbol(targetVariable))
                {
                    // Target variable is on the right side, swap operands and try to isolate
                    Equality newEquation = IsolateSide(currentEquation.RightOperand, currentEquation.LeftOperand, targetVariable, out isolationOccurred);
                    if (isolationOccurred)
                    {
                        currentEquation = newEquation; // This newEquation is already canonicalized within IsolateSide
                        changedInIteration = true;
                    }
                }
                else if (currentEquation.LeftOperand.ContainsSymbol(targetVariable) && currentEquation.RightOperand.ContainsSymbol(targetVariable))
                {
                    // Target variable is on both sides. Rearrange to (Left - Right = 0)
                    IExpression rearrangedLeft = new Add(currentEquation.LeftOperand, new Multiply(new Number(-1m), currentEquation.RightOperand).Canonicalize()).Canonicalize();
                    IExpression zero = new Number(0m);
                    
                    Equality rearrangedEquation = new Equality(rearrangedLeft, zero);
                    if (!rearrangedEquation.InternalEquals(currentEquation))
                    {
                        currentEquation = rearrangedEquation;
                        changedInIteration = true;
                        isolationOccurred = true; // Mark as changed by isolation (rearrangement)
                    }
                }
                else
                {
                    // Neither side contains the target variable.
                    // If the equation simplifies to a tautology (e.g., 5 = 5), it means X can be anything, but we haven't isolated X.
                    // If it simplifies to a contradiction (e.g., 5 = 6), there's no solution.
                    // In both cases, if the target variable is gone, this strategy cannot 'isolate' it into `targetVariable = solution` form.
                    return SolveResult.Failure(currentEquation, $"Failed to solve. Target variable '{targetVariable.ToDisplayString()}' could not be isolated.", traceBuilder?.ToImmutable());
                }

                if (isolationOccurred)
                {
                    // Add trace if isolation occurred and it was not captured by simplification in previous iteration
                    if (!currentEquation.InternalEquals(previousEquation)) 
                    {
                        traceBuilder?.Add(currentEquation);
                    }
                }

                // Step 4: Check if goal is achieved after isolation
                if (CheckGoal(currentEquation, targetVariable))
                {
                    return SolveResult.Success(currentEquation, "Equation solved successfully.", traceBuilder?.ToImmutable());
                }

                // Step 5: Check for stagnation
                // If neither simplification nor isolation made progress in this iteration
                if (!changedInIteration)
                {
                    return SolveResult.Failure(currentEquation, $"Failed to solve. No further progress achievable. Final expression: {currentEquation.ToDisplayString()}", traceBuilder?.ToImmutable());
                }
            }

            // Exited loop, max iterations reached
            return SolveResult.Failure(currentEquation, $"Max iterations ({context.MaxIterations}) reached before full solution.", traceBuilder?.ToImmutable());
        }

        /// <summary>
        /// Checks if the equation is in the desired solved form: TargetVariable = Solution (where solution does not contain TargetVariable).
        /// </summary>
        private static bool CheckGoal(Equality equation, Symbol targetVariable)
        {
            // Case 1: LeftOperand is target, RightOperand does not contain target
            if (equation.LeftOperand.InternalEquals(targetVariable) && !equation.RightOperand.ContainsSymbol(targetVariable))
            {
                return true;
            }
            // Case 2: RightOperand is target, LeftOperand does not contain target
            if (equation.RightOperand.InternalEquals(targetVariable) && !equation.LeftOperand.ContainsSymbol(targetVariable))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to perform one step of isolation on the 'targetSide' expression, assuming it contains the target variable
        /// and 'otherSide' does not. It applies the inverse of the outermost operation on 'targetSide'.
        /// </summary>
        /// <param name="targetSideExpression">The expression on the side of the equation containing the target variable.</param>
        /// <param name="otherSideExpression">The expression on the other side of the equation (assumed not to contain the target variable).</param>
        /// <param name="targetVariable">The symbol being isolated.</param>
        /// <param name="changed">Outputs true if an isolation step was performed, false otherwise.</param>
        /// <returns>A new Equality expression after an isolation step (target_expr = other_expr), or the original (target_expr = other_expr) if no step was taken.</returns>
        private static Equality IsolateSide(IExpression targetSideExpression, IExpression otherSideExpression, Symbol targetVariable, out bool changed)
        {
            changed = false;
            IExpression newTargetSide = targetSideExpression;
            IExpression newOtherSide = otherSideExpression;

            // Ordered by PEMDAS-reverse to peel outermost operations first (Add/Subtract, Multiply/Divide, Power, Function)

            // Isolation for Addition/Subtraction (A + B = C => A = C - B)
            if (newTargetSide is Add addOp)
            {
                IExpression? termWithTarget = null;
                IExpression? otherTermsSum = null; 
                foreach (IExpression arg in addOp.Arguments)
                {
                    if (arg.ContainsSymbol(targetVariable))
                    {
                        // Multiple terms containing the target variable are not handled by simple isolation - keep the whole sum
                        if (termWithTarget is not null && !termWithTarget.InternalEquals(addOp)) 
                        {
                            termWithTarget = addOp; // Mark the whole operation as the 'term with target' if multiple exist.
                            break;
                        }
                        termWithTarget = arg;
                    }
                    else
                    {
                        if (otherTermsSum is null) { otherTermsSum = arg; }
                        else { otherTermsSum = new Add(otherTermsSum, arg).Canonicalize(); }
                    }
                }
                
                if (termWithTarget is not null && !termWithTarget.InternalEquals(addOp) && otherTermsSum is not null)
                {
                    newTargetSide = termWithTarget;
                    // Move otherTermsSum to the other side by subtracting it
                    newOtherSide = new Add(otherSideExpression.Canonicalize(), new Multiply(new Number(-1m), otherTermsSum.Canonicalize()).Canonicalize()).Canonicalize();
                    changed = true;
                }
            }
            // Isolation for Multiplication/Division (A * B = C => A = C / B)
            else if (newTargetSide is Multiply multiplyOp)
            {
                IExpression? factorWithTarget = null;
                IExpression? otherFactorsProduct = null;
                foreach (IExpression arg in multiplyOp.Arguments)
                {
                    if (arg.ContainsSymbol(targetVariable))
                    {
                        if (factorWithTarget is not null && !factorWithTarget.InternalEquals(multiplyOp)) 
                        {
                            factorWithTarget = multiplyOp;
                            break;
                        }
                        factorWithTarget = arg;
                    }
                    else
                    {
                        if (otherFactorsProduct is null) { otherFactorsProduct = arg; }
                        else { otherFactorsProduct = new Multiply(otherFactorsProduct, arg).Canonicalize(); }
                    }
                }

                if (factorWithTarget is not null && !factorWithTarget.InternalEquals(multiplyOp) && otherFactorsProduct is not null)
                {
                    newTargetSide = factorWithTarget;
                    // Move otherFactorsProduct to the other side by dividing by it
                    if (otherFactorsProduct is Number factorNum && factorNum.Value == 0m)
                    {
                        changed = false; // Cannot divide by zero
                        return new Equality(targetSideExpression, otherSideExpression);
                    }
                    IExpression inverseFactor = new Power(otherFactorsProduct.Canonicalize(), new Number(-1m)).Canonicalize();
                    newOtherSide = new Multiply(otherSideExpression.Canonicalize(), inverseFactor).Canonicalize();
                    changed = true;
                }
            }
            // Isolation for Power operation (A^B = C)
            else if (newTargetSide is Power powerOp)
            {
                IExpression @base = powerOp.Base;
                IExpression exponent = powerOp.Exponent;

                if (@base.ContainsSymbol(targetVariable) && !exponent.ContainsSymbol(targetVariable))
                {
                    // Case: Base contains target (X^N = Y => X = Y^(1/N))
                    newTargetSide = @base;
                    if (exponent is Number expNum && expNum.Value == 0m)
                    {
                        changed = false; // Cannot take 1/0 exponent
                        return new Equality(targetSideExpression, otherSideExpression);
                    }
                    IExpression inverseExponent = new Power(exponent.Canonicalize(), new Number(-1m)).Canonicalize();
                    newOtherSide = new Power(otherSideExpression.Canonicalize(), inverseExponent).Canonicalize();
                    changed = true;
                }
                else if (exponent.ContainsSymbol(targetVariable) && !@base.ContainsSymbol(targetVariable))
                {
                    // Case: Exponent contains target (N^X = Y => X = Log(Y, N))
                    newTargetSide = exponent;
                    newOtherSide = new Function("Log", ImmutableList.Create(otherSideExpression.Canonicalize(), @base.Canonicalize())).Canonicalize();
                    changed = true;
                }
            }
            // Isolation for Function operation (fun(X) = Y)
            else if (newTargetSide is Function funcOp)
            {
                if (funcOp.Arguments.Count == 1 && funcOp.Arguments[0].ContainsSymbol(targetVariable))
                {
                    string funcName = funcOp.Name.ToLowerInvariant();
                    string inverseFuncName = string.Empty;
                    bool handledBySpecialCase = false;

                    // Standard trigonometric and exponential function inverses
                    if (funcName == "sin") inverseFuncName = "asin";
                    else if (funcName == "cos") inverseFuncName = "acos";
                    else if (funcName == "tan") inverseFuncName = "atan";
                    else if (funcName == "exp") inverseFuncName = "log"; // Natural logarithm
                    else if (funcName == "log") // Assuming natural log
                    {
                        // For log(A) = B => A = exp(B)
                        newTargetSide = funcOp.Arguments[0];
                        newOtherSide = new Function("exp", ImmutableList.Create(otherSideExpression.Canonicalize())).Canonicalize();
                        changed = true;
                        handledBySpecialCase = true;
                    }
                    else if (funcName == "log" && funcOp.Arguments.Count == 2) // Log(value, base) case
                    {
                        // If Log(value with target, base without target) = result => value = base^result
                        if (funcOp.Arguments[0].ContainsSymbol(targetVariable) && !funcOp.Arguments[1].ContainsSymbol(targetVariable))
                        {
                            newTargetSide = funcOp.Arguments[0];
                            newOtherSide = new Power(funcOp.Arguments[1].Canonicalize(), otherSideExpression.Canonicalize()).Canonicalize();
                            changed = true;
                            handledBySpecialCase = true;
                        }
                    }

                    if (changed == false && handledBySpecialCase == false && !string.IsNullOrEmpty(inverseFuncName)) // Only if not handled by a special case above
                    {
                        newTargetSide = funcOp.Arguments[0];
                        newOtherSide = new Function(inverseFuncName, ImmutableList.Create(otherSideExpression.Canonicalize())).Canonicalize();
                        changed = true;
                    }
                }
            }

            // If no specific isolation rule applied, or if the target is already `targetVariable`
            if (!changed)
            {
                // No isolation performed, return the original equation
                return new Equality(targetSideExpression, otherSideExpression);
            }
            
            // Return the new equation (target_variable_isolated = other_side_transformed_and_canonicalized)
            return new Equality(newTargetSide, newOtherSide);
        }
    }
}