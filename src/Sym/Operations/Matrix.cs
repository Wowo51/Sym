//Copyright Warren Harding 2025.
using Sym.Core;
using Sym.Atoms;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Sym.Operations
{
    /// <summary>
    /// Represents a matrix as an ordered collection of its scalar components.
    /// Assumes elements are scalar expressions.
    /// </summary>
    public sealed class Matrix : Operation
    {
        /// <summary>
        /// The dimensions of the matrix (rows, columns).
        /// </summary>
        public ImmutableArray<int> MatrixDimensions { get; init; }

        public Matrix(ImmutableArray<int> dimensions, ImmutableList<IExpression> components) 
            : base(components)
        {
            if (dimensions.Length != 2)
            {
                // This constructor specifically for 2D matrices.
                // Should use Shape.Error or throw, but we avoid throw statements.
                MatrixDimensions = ImmutableArray.Create(0,0); 
                // Currently, this will create an invalid matrix. Shape property will handle IsValid.
                return;
            }
            MatrixDimensions = dimensions;

            // Check if the total number of components matches the dimensions
            if (components.Count != dimensions[0] * dimensions[1])
            {
                // Assign a dummy dimension or handle as invalid, as we don't throw.
                // Shape property will reflect invalidity.
                MatrixDimensions = ImmutableArray.Create(0,0); // Mark as invalid internally if counts don't match
            }
            
            // Further validation for components being scalar could be added here,
            // but for simplicity, let the Shape property derive it.
            // If any component is not scalar, the matrix shape itself will be Error.
        }

        /// <summary>
        /// Constructor for creating a matrix from a list of rows represented as vectors.
        /// </summary>
        /// <param name="rows">A list of vector expressions, where each vector is a row of the matrix.</param>
        public Matrix(ImmutableList<Vector> rows) : base(FlattenVectorsToComponents(rows))
        {
            if (rows.IsEmpty)
            {
                MatrixDimensions = ImmutableArray.Create(0, 0);
                return;
            }

            int numRows = rows.Count;
            int numCols = rows[0].Arguments.Count;

            // All rows must have the same number of columns
            if (rows.Any(r => r.Arguments.Count != numCols))
            {
                MatrixDimensions = ImmutableArray.Create(0, 0); // Invalid matrix dimensions
                return;
            }

            MatrixDimensions = ImmutableArray.Create(numRows, numCols);
        }

        private static ImmutableList<IExpression> FlattenVectorsToComponents(ImmutableList<Vector> rows)
        {
            ImmutableList<IExpression>.Builder components = ImmutableList.CreateBuilder<IExpression>();
            foreach (Vector row in rows)
            {
                components.AddRange(row.Arguments);
            }
            return components.ToImmutable();
        }

        public override Shape Shape
        {
            get
            {
                if (MatrixDimensions.Length != 2 || MatrixDimensions[0] * MatrixDimensions[1] != Arguments.Count)
                {
                    return Shape.Error; // Invalid matrix construction
                }

                // All components must be scalar expressions for it to be a valid matrix.
                if (Arguments.Any(arg => !arg.Shape.IsScalar))
                {
                    return Shape.Error;
                }
                return new Shape(MatrixDimensions);
            }
        }

        public override IExpression Canonicalize()
        {
            ImmutableList<IExpression> canonicalArgs = Arguments.Select(arg => arg.Canonicalize()).ToImmutableList();

            bool changed = false;
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!ReferenceEquals(Arguments[i], canonicalArgs[i])) 
                {
                    changed = true;
                    break;
                }
            }

            if (changed)
            {
                return new Matrix(MatrixDimensions, canonicalArgs);
            }
            return this;
        }

        public override string ToDisplayString()
        {
            if (Shape.IsValid)
            {
                int rows = MatrixDimensions[0];
                int cols = MatrixDimensions[1];
                System.Collections.Generic.List<string> rowStrings = new System.Collections.Generic.List<string>();
                for (int i = 0; i < rows; i++)
                {
                    System.Collections.Generic.IEnumerable<string> rowComponents = Arguments
                        .Skip(i * cols)
                        .Take(cols)
                        .Select(arg => arg.ToDisplayString());
                    rowStrings.Add($"[{string.Join(", ", rowComponents)}]");
                }
                return $"Matrix({rows}x{cols})<{string.Join("; ", rowStrings)}>";
            }
            return "Matrix(Invalid)";
        }

        public override bool InternalEquals(IExpression other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is not Matrix otherMatrix || !MatrixDimensions.SequenceEqual(otherMatrix.MatrixDimensions))
            {
                return false;
            }

            if (Arguments.Count != otherMatrix.Arguments.Count)
            {
                return false;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].InternalEquals(otherMatrix.Arguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int InternalGetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetType());
            foreach (int dim in MatrixDimensions)
            {
                hash.Add(dim);
            }
            foreach (IExpression arg in Arguments)
            {
                hash.Add(arg);
            }
            return hash.ToHashCode();
        }

        public override Operation WithArguments(ImmutableList<IExpression> newArgs)
        {
            // When WithArguments is called, it assumes the dimensions are still valid based on the original object.
            return new Matrix(MatrixDimensions, newArgs);
        }
    }
}