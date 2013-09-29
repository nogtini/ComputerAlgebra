﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LinqExpression = System.Linq.Expressions.Expression;

namespace SyMath
{
    /// <summary>
    /// Function defined by a lookup table with linear interpolation.
    /// </summary>
    public class LutFunction : Function
    {
        private List<Variable> args;
        public IEnumerable<Variable> Arguments { get { return args; } }

        private SortedDictionary<double, double> points = new SortedDictionary<double, double>();

        private LutFunction(string Name, IEnumerable<Arrow> Points) : base(Name) 
        {
            args = new List<Variable>() { Variable.New("x1") };
            foreach (Arrow i in Points)
                points.Add((double)i.Left, (double)i.Right);
        }

        public static LutFunction New(string Name, IEnumerable<Arrow> Points) { return new LutFunction(Name, Points); }

        public override Expression Call(IEnumerable<Expression> Params)
        {
            double x = (double)Params.Single();

            throw new NotImplementedException();
        }

        public override bool CanCall(IEnumerable<Expression> Params)
        {
            return Arguments.Count() == Params.Count();
        }
    }
}