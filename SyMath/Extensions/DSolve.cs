﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyMath
{
    public enum IntegrationMethod
    {
        Euler,
        BackwardEuler,
        Trapezoid
    }

    /// <summary>
    /// Extensions for solving equations.
    /// </summary>
    public static class DSolveExtension
    {
        /// <summary>
        /// Solve a linear system of differential equations with initial conditions using the laplace transform.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="y"></param>
        /// <param name="y0"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Arrow> DSolve(this IEnumerable<Equal> f, IEnumerable<Expression> y, IEnumerable<Arrow> y0, Expression t)
        {
            // TODO: Add missing initial conditions as constants.
            List<Arrow> C = new List<Arrow>();

            // Find F(s) = L[f(t)] and substitute the initial conditions.
            List<Equal> F = f.Select(i => Equal.New(
                L(i.Left, t).Evaluate(y0).Evaluate(C),
                L(i.Right, t).Evaluate(y0).Evaluate(C))).ToList();

            // Solve F for Y(s) = L[y(t)].
            List<Arrow> Y = F.Solve(y.Select(i => L(i, t)));

            // Take L^-1[Y].
            Y = Y.Select(i => Arrow.New(IL(i.Left, t), IL(i.Right, t))).ToList();
            if (Y.Any(i => i.IsFunctionOf(s)))
                throw new InvalidOperationException("DSolve failed");

            return Y;
        }

        private static List<Arrow> NDSolve(this List<Equal> f, List<Expression> y, Expression t, Expression t0, Expression h, IntegrationMethod method, int iterations)
        {
            // Find y' in terms of y.
            List<Arrow> dydt = f.Solve(y.Select(i => D(i, t)));

            // Euler step, used as an initial guess for other methods.
            // y[t] = y[t0] + h*f[t0, y[t0]]
            List<Arrow> step = dydt.Select(i => Arrow.New(
                DOf(i.Left),
                DOf(i.Left).Evaluate(t, t0) + h * i.Right.Evaluate(t, t0))).ToList();

            switch (method)
            {
                case IntegrationMethod.Euler:
                    break;
                case IntegrationMethod.BackwardEuler:
                    // Solve y[t] = y[t0] + h*f[t, y[t]] for y[t].
                    step = NSolveExtension.NSolve(
                        dydt.Select(i => Equal.New(
                            DOf(i.Left),
                            DOf(i.Left).Evaluate(t, t0) + h * i.Right)),
                        step,
                        iterations);
                    break;
                case IntegrationMethod.Trapezoid:
                    // Solve y[t] = y[t0] + (h/2)*(f[t0, y[t0]] + f[t, y[t]]) for y[t].
                    step = NSolveExtension.NSolve(
                        dydt.Select(i => Equal.New(
                            DOf(i.Left),
                            DOf(i.Left).Evaluate(t, t0) + (h / 2) * (i.Right.Evaluate(t, t0) + i.Right))),
                        step,
                        iterations);
                    break;
                default:
                    throw new NotImplementedException(method.ToString());
            }
            return step;
        }

        /// <summary>
        /// Solve a linear system of differential equations for y[t] at t = t0 + h in terms of y[t0].
        /// </summary>
        /// <param name="f">Equations to solve.</param>
        /// <param name="y">Functions to solve for.</param>
        /// <param name="t">Independent variable.</param>
        /// <param name="t0">Previous timestep.</param>
        /// <param name="h">Step size.</param>
        /// <param name="method">Integration method to use for differential equations.</param>
        /// <param name="iterations">Iterations to use for numerical solutions.</param>
        /// <returns>Expressions for y[t0 + h].</returns>
        public static List<Arrow> NDSolve(this IEnumerable<Equal> f, IEnumerable<Expression> y, Expression t, Expression t0, Expression h, IntegrationMethod method, int iterations)
        {
            return NDSolve(f.ToList(), y.ToList(), t, t0, h, method, iterations);
        }
                
        // Get the expression that x is a derivative of.
        private static Expression DOf(Expression x)
        {
            Call d = (Call)x;
            if (d.Target.Name == "D")
                return d.Arguments.First();
            throw new InvalidOperationException("Expression is not a derivative");
        }

        // Helpers.
        private static Expression s = Variable.New("_s");
        private static Expression L(Expression f, Expression t) { return f.LaplaceTransform(t, s); }
        private static Expression IL(Expression f, Expression t) { return f.InverseLaplaceTransform(s, t); }

        private static Expression D(Expression f, Expression t) { return f.Differentiate(t); }
    }
}