//
// SVD.cs
// MetrologyCalibration - Core Math Library
//
// Created by AI Assistant on 2023-10-27.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics; // For Vector3 and Matrix4x4

// ---
// 1. Matrix3x3 Struct
// ---

/// <summary>
/// Custom 3x3 Matrix struct for SVD operations, as System.Numerics.Matrix4x4 doesn't have a 3x3 variant.
/// </summary>
public struct Matrix3x3
{
    public float M11, M12, M13;
    public float M21, M22, M23;
    public float M31, M32, M33;

    public static Matrix3x3 Identity() { return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1); }
    public static Matrix3x3 Zero() { return new Matrix3x3(0, 0, 0, 0, 0, 0, 0, 0, 0); }

    public Matrix3x3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
    {
        M11 = m11; M12 = m12; M13 = m13;
        M21 = m21; M22 = m22; M23 = m23;
        M31 = m31; M32 = m32; M33 = m33;
    }

    // Indexer for convenience (0-indexed row, column)
    public float this[int row, int col]
    {
        get
        {
            switch (row)
            {
                case 0: switch (col) { case 0: return M11; case 1: return M12; case 2: return M13; } break;
                case 1: switch (col) { case 0: return M21; case 1: return M22; case 2: return M23; } break;
                case 2: switch (col) { case 0: return M31; case 1: return M32; case 2: return M33; } break;
            }
            throw new IndexOutOfRangeException("Matrix index out of bounds.");
        }
        set
        {
            switch (row)
            {
                case 0: switch (col) { case 0: M11 = value; return; case 1: M12 = value; return; case 2: M13 = value; return; } break;
                case 1: switch (col) { case 0: M21 = value; return; case 1: M22 = value; return; case 2: M23 = value; return; } break;
                case 2: switch (col) { case 0: M31 = value; return; case 1: M32 = value; return; case 2: M33 = value; return; } break;
            }
            throw new IndexOutOfRangeException("Matrix index out of bounds.");
        }
    }

    public Matrix3x3 Transpose()
    {
        return new Matrix3x3(
            M11, M21, M31,
            M12, M22, M32,
            M13, M23, M33
        );
    }

    public float Determinant()
    {
        return M11 * (M22 * M33 - M23 * M32)
             - M12 * (M21 * M33 - M23 * M31)
             + M13 * (M21 * M32 - M22 * M31);
    }

    // Matrix multiplication
    public static Matrix3x3 operator *(Matrix3x3 left, Matrix3x3 right)
    {
        Matrix3x3 result = new Matrix3x3();
        result.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31;
        result.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32;
        result.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33;

        result.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31;
        result.M22 = left.M21 * right.M12 + left.M22 * right.R22 + left.M23 * right.M32;
        result.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33;

        result.M31 = left.M31 * right.M11 + left.M32 * right.R21 + left.M33 * right.M31;
        result.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32;
        result.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33;
        return result;
    }

    // Matrix-Vector multiplication
    public static Vector3 Multiply(Matrix3x3 matrix, Vector3 vector)
    {
        return new Vector3(
            matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z,
            matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z,
            matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z
        );
    }

    // Matrix addition
    public static Matrix3x3 operator +(Matrix3x3 left, Matrix3x3 right)
    {
        return new Matrix3x3(
            left.M11 + right.M11, left.M12 + right.M12, left.M13 + right.M13,
            left.M21 + right.M21, left.M22 + right.M22, left.M23 + right.M23,
            left.M31 + right.M31, left.M32 + right.M32, left.M33 + right.M33
        );
    }

    // Outer product (Vector3 * Vector3.Transpose)
    public static Matrix3x3 OuterProduct(Vector3 v1, Vector3 v2)
    {
        return new Matrix3x3(
            v1.X * v2.X, v1.X * v2.Y, v1.X * v2.Z,
            v1.Y * v2.X, v1.Y * v2.Y, v1.Y * v2.Z,
            v1.Z * v2.X, v1.Z * v2.Y, v1.Z * v2.Z
        );
    }
}

// ---
// 2. SVD Result Struct
// ---

/// <summary>
/// Result struct for Singular Value Decomposition (SVD). A = U * diag(S) * V.Transpose()
/// </summary>
public struct SVDResult
{
    public Matrix3x3 U;
    public Vector3 S_diag; // Singular values on the diagonal (s1, s2, s3)
    public Matrix3x3 V;
}

// ---
// 3. SVD Class (with Jacobi Eigenvalue Implementation)
// ---

/// <summary>
/// Provides Singular Value Decomposition (SVD) for 3x3 matrices using the Eigenvalue Decomposition of A^T * A.
/// </summary>
public static class SVD
{
    private const int MaxIterations = 50; // Max iterations for Jacobi method

    /// <summary>
    /// Computes the Singular Value Decomposition (SVD) of a 3x3 matrix A.
    /// A = U * diag(S) * V.Transpose()
    /// </summary>
    public static SVDResult Compute(Matrix3x3 A)
    {
        // Step 1: Compute the symmetric matrix A^T * A
        Matrix3x3 ATA = A.Transpose() * A;

        // Step 2: Eigenvalue decomposition of A^T * A.
        var eigenATA = EigenvalueDecompositionSymmetric(ATA);
        Matrix3x3 V_matrix = eigenATA.Eigenvectors;
        Vector3 S_squared_diag = eigenATA.Eigenvalues;

        // Step 3: Compute singular values S = sqrt(S^2)
        Vector3 S_diag = new Vector3(
            (float)Math.Sqrt(Math.Max(0, S_squared_diag.X)), // Ensure non-negative under sqrt
            (float)Math.Sqrt(Math.Max(0, S_squared_diag.Y)),
            (float)Math.Sqrt(Math.Max(0, S_squared_diag.Z))
        );

        // Step 4: Sort singular values and corresponding vectors in descending order
        var sortedEigen = SortEigen(S_diag, V_matrix);
        S_diag = sortedEigen.Item1;
        V_matrix = sortedEigen.Item2;

        // Step 5: Compute U matrix.
        // U = A * V * S_inverse. For non-zero S[i], U_i = (A * V_i) / S[i].
        float threshold = 1e-6f;
        Matrix3x3 U_matrix = Matrix3x3.Zero();

        for (int i = 0; i < 3; i++)
        {
            Vector3 v_col_i = new Vector3(V_matrix[0, i], V_matrix[1, i], V_matrix[2, i]);

            if (S_diag[i] > threshold)
            {
                // U_i = (A * V_i) / S_i
                Vector3 u_col_i = Matrix3x3.Multiply(A, v_col_i) / S_diag[i];
                
                U_matrix[0, i] = u_col_i.X;
                U_matrix[1, i] = u_col_i.Y;
                U_matrix[2, i] = u_col_i.Z;
            }
            // Rank-deficient case (S_diag[i] <= threshold) is handled by subsequent Orthogonalize step.
        }

        // Step 6: Ensure U is orthonormal (robustly handles rank-deficient cases via Gram-Schmidt)
        U_matrix = Orthogonalize(U_matrix);

        return new SVDResult { U = U_matrix, S_diag = S_diag, V = V_matrix };
    }

    /// <summary>
    /// Helper to compute eigenvalues and eigenvectors of a 3x3 symmetric matrix M
    /// using the Jacobi method. M = V * D * V.Transpose().
    /// </summary>
    private static (Vector3 Eigenvalues, Matrix3x3 Eigenvectors) EigenvalueDecompositionSymmetric(Matrix3x3 M)
    {
        // Jacobi Iteration Method for 3x3 symmetric matrices
        Matrix3x3 V = Matrix3x3.Identity();
        Matrix3x3 A = M;
        float tolerance = 1e-9f;

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            // Find the largest off-diagonal element (p, q)
            float a12 = A[0, 1]; float a13 = A[0, 2]; float a23 = A[1, 2];
            
            float maxOffDiagSq = a12 * a12 + a13 * a13 + a23 * a23;
            if (maxOffDiagSq < tolerance) { break; } // Converged

            int p = 0, q = 1;
            float maxOffDiag = Math.Abs(a12);

            if (Math.Abs(a13) > maxOffDiag) { maxOffDiag = Math.Abs(a13); p = 0; q = 2; }
            if (Math.Abs(a23) > maxOffDiag) { p = 1; q = 2; }

            // Determine rotation angle (phi) to zero out A[p, q] and A[q, p]
            float app = A[p, p];
            float aqq = A[q, q];
            float apq = A[p, q];
            
            float tan2Phi;
            if (Math.Abs(app - aqq) < 1e-12f)
            {
                tan2Phi = (float)Math.PI / 4.0f; // 45 degrees
            }
            else
            {
                tan2Phi = 2.0f * apq / (app - aqq);
            }

            // Robust calculation of tan(phi) from tan(2*phi)
            float t = (float)Math.Abs(tan2Phi) < 1e-12f ? 0.0f : (float)Math.Sign(tan2Phi) * ((float)Math.Sqrt(tan2Phi * tan2Phi + 1.0f) - 1.0f) / tan2Phi;
            float cosPhi = 1.0f / (float)Math.Sqrt(t * t + 1.0f);
            float sinPhi = t * cosPhi;
            
            // Construct the Jacobi Rotation Matrix J
            Matrix3x3 J = Matrix3x3.Identity();
            J[p, p] = cosPhi;
            J[q, q] = cosPhi;
            J[p, q] = -sinPhi;
            J[q, p] = sinPhi;

            // Update A: A_new = J^T * A * J (J^T is J(p,q, -phi))
            A = J.Transpose() * A * J;
            
            // Update the Eigenvector Matrix V: V_new = V * J
            V = V * J;
        }

        Vector3 eigenvalues = new Vector3(A[0, 0], A[1, 1], A[2, 2]);

        return (eigenvalues, V);
    }

    /// <summary>
    /// Helper to perform Gram-Schmidt orthogonalization on a 3x3 matrix's columns.
    /// Ensures the resulting matrix is orthonormal.
    /// </summary>
    private static Matrix3x3 Orthogonalize(Matrix3x3 M)
    {
        // Extract columns
        Vector3 c0 = new Vector3(M[0, 0], M[1, 0], M[2, 0]);
        Vector3 c1 = new Vector3(M[0, 1], M[1, 1], M[2, 1]);
        // c2 column will be calculated as cross product

        // Normalize c0
        if (c0.LengthSquared() < 1e-12f) c0 = Vector3.UnitX; // Fallback if zero vector
        c0 = Vector3.Normalize(c0);

        // Orthogonalize c1 relative to c0, then normalize
        c1 = c1 - Vector3.Dot(c1, c0) * c0;
        if (c1.LengthSquared() < 1e-12f)
        {
            // If c1 is linearly dependent on c0, pick a vector perpendicular to c0
            if (Math.Abs(Vector3.Dot(Vector3.UnitX, c0)) < 0.5f) c1 = Vector3.UnitX;
            else c1 = Vector3.UnitY;
            c1 = c1 - Vector3.Dot(c1, c0) * c0; // Ensure orthogonality again
        }
        c1 = Vector3.Normalize(c1);

        // Find c2 as the cross product of c0 and c1 (ensures orthogonality and determinant)
        Vector3 c2 = Vector3.Normalize(Vector3.Cross(c0, c1));

        // Reassemble matrix
        return new Matrix3x3(
            c0.X, c1.X, c2.X,
            c0.Y, c1.Y, c2.Y,
            c0.Z, c1.Z, c2.Z
        );
    }

    /// <summary>
    /// Sorts eigenvalues (singular values) in descending order and reorders eigenvectors accordingly.
    /// </summary>
    private static Tuple<Vector3, Matrix3x3> SortEigen(Vector3 eigenvalues, Matrix3x3 eigenvectors)
    {
        var eigenPairs = new (float Value, Vector3 Vector)[3];
        for (int i = 0; i < 3; i++)
        {
            eigenPairs[i] = (eigenvalues[i], new Vector3(eigenvectors[0, i], eigenvectors[1, i], eigenvectors[2, i]));
        }

        // Sort in descending order of value
        Array.Sort(eigenPairs, (p1, p2) => p2.Value.CompareTo(p1.Value));

        Vector3 sortedEigenvalues = new Vector3(eigenPairs[0].Value, eigenPairs[1].Value, eigenPairs[2].Value);
        Matrix3x3 sortedEigenvectors = Matrix3x3.Zero();

        // Re-assemble the V matrix with sorted columns
        for (int i = 0; i < 3; i++)
        {
            sortedEigenvectors[0, i] = eigenPairs[i].Vector.X;
            sortedEigenvectors[1, i] = eigenPairs[i].Vector.Y;
            sortedEigenvectors[2, i] = eigenPairs[i].Vector.Z;
        }

        return Tuple.Create(sortedEigenvalues, sortedEigenvectors);
    }
}