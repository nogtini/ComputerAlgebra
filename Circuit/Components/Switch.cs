﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SyMath;
using System.ComponentModel;

namespace Circuit
{ 
    /// <summary>
    /// Resistor is a linear component with V = R*i.
    /// </summary>
    [CategoryAttribute("Controls")]
    [DisplayName("Switch")]
    public class Switch : TwoTerminal
    {
        protected bool closed = false;
        [Description("Closed = true corresponds to a closed circuit.")]
        [SchematicPersistent]
        [SimulationParameter]
        public bool Closed { get { return closed; } set { closed = value; NotifyChanged("Closed"); } }

        public Switch() { Name = "S1"; }
        public Switch(bool Closed) : this() { closed = Closed; }

        protected override void Analyze(IList<Equal> Kcl)
        {
            if (closed)
            {
                Kcl.Add(Equal.New(Anode.V, Cathode.V));
            }
            else
            {
                Anode.i = Cathode.i = 0;
            }
        }

        protected override void DrawSymbol(SymbolLayout Sym)
        {
            Sym.AddWire(Anode, new Coord(0, 12));
            Sym.AddWire(Cathode, new Coord(0, -12));
            Sym.AddCircle(ShapeType.Black, new Coord(0, 12), 2);
            Sym.AddCircle(ShapeType.Black, new Coord(0, -12), 2);
            Sym.InBounds(new Coord(-10, 0), new Coord(10, 0));

            if (closed)
                Sym.AddWire(new Coord(0, -12), new Coord(0, 12));
            else
                Sym.AddWire(new Coord(0, -12), new Coord(-8, 10));

            Sym.DrawText(Name, new Coord(2, 0), Alignment.Near, Alignment.Center);
        }
    }
}