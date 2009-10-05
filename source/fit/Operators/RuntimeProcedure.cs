﻿// Copyright © 2009 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fitSharp.Fit.Engine;
using fitSharp.Fit.Model;
using fitSharp.Fit.Operators;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fit.Operators {
    public class RuntimeProcedure: CellOperator, RuntimeOperator<Cell>
    {
        public bool CanCreate(string memberName, Tree<Cell> parameters) { return false; }

        public TypedValue Create(string memberName, Tree<Cell> parameters) { return TypedValue.Void; }

        public bool CanInvoke(TypedValue instance, string memberName, Tree<Cell> parameters) {
            return Processor.Contains(new Procedure(memberName));
        }

        public TypedValue Invoke(TypedValue instance, string memberName, Tree<Cell> parameters) {
            var procedure = Processor.Load(new Procedure(memberName));
            return Invoke((Parse)procedure.Instance, instance, parameters);
        }

        private TypedValue Invoke(Parse procedure, TypedValue target, Tree<Cell> parameterValues) {
            var doFixture = new CellTree(new CellTree("dofixture"));
            var fixture = Processor.Parse(typeof (Interpreter), target, doFixture);

            var parameters = new Parameters(procedure.Parts, parameterValues);
            Parse body = procedure.DeepCopy(
                parameters.Substitute,
                s => s == procedure ? null : s.More,
                s => s == procedure ? s.Parts.More : s.Parts);

            TypedValue result = Processor.Execute(fixture, new CellTree((Tree<Cell>)body));

            Processor.TestStatus.LastAction = Processor.Parse(typeof(StoryTestString), TypedValue.Void, body).ValueString;
            return result;
        }

        private class Parameters {
            private readonly Tree<Cell> names;
            private readonly Tree<Cell> values;
           
            public Parameters(Tree<Cell> names, Tree<Cell> values) {
                this.names = names;
                this.values = values;
            }

            public Parse Substitute(Parse source) {
                int i = 2;
                foreach (Tree<Cell> parameterValue in values.Branches) {
                    if (names.Branches[i].Value.Text == source.Value.Text) {
                        return ((Parse) parameterValue).DeepCopy();
                    }
                    i += 2;
                }
                return null;
            }
        }
    }
}