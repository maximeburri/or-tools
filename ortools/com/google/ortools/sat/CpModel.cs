// Copyright 2010-2017 Google
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Google.OrTools.Sat
{
using System;
using System.Collections.Generic;

// Holds an integer expression.
public class IntegerExpression
{

  public int Index
  {
    get { return GetIndex(); }
  }

  public virtual int GetIndex()
  {
    throw new NotImplementedException();
  }

}

class ProductCst : IntegerExpression
{

}

class SumArray : IntegerExpression
{

}

public class IntVar : IntegerExpression
{
  public IntVar(CpModelProto model, IEnumerable<long> bounds, string name,
         int is_present_index) {
    model_ = model;
    index_ = model.Variables.Count;
    var_ = new IntegerVariableProto();
    var_.Name = name;
    var_.Domain.Add(bounds);
    var_.EnforcementLiteral.Add(is_present_index);
    model.Variables.Add(var_);
  }

  public IntVar(CpModelProto model, IEnumerable<long> bounds, string name) {
    model_ = model;
    index_ = model.Variables.Count;
    var_ = new IntegerVariableProto();
    var_.Name = name;
    var_.Domain.Add(bounds);
    model.Variables.Add(var_);
  }

  public override int GetIndex()
  {
    return index_;
  }


  private CpModelProto model_;
  private int index_;
  private List<long> bounds_;
  private IntegerVariableProto var_;
}

class NotBooleanVariable : IntegerExpression
{
  NotBooleanVariable(IntVar boolvar)
  {
    boolvar_ = boolvar;
  }

  public override int GetIndex()
  {
    return -boolvar_.Index - 1;
  }

  IntVar Not()
  {
    return boolvar_;
  }

  private IntVar boolvar_;
}

class Product : IntegerExpression
{

}

public class BoundIntegerExpression
{

}

public class Constraint
{
  public Constraint(CpModelProto model)
  {
    index_ = model.Constraints.Count;
    constraint_ = new ConstraintProto();
    model.Constraints.Add(constraint_);
  }

  public void OnlyEnforceIf(IntegerExpression lit)
  {
    constraint_.EnforcementLiteral.Add(lit.Index);
  }

  public int Index
  {
    get  { return index_; }
  }

  public ConstraintProto Proto()
  {
    return constraint_;
  }

  private int index_;
  private ConstraintProto constraint_;
}

class IntervalVar
{

}

public class CpModel
{
  public CpModel()
  {
    model_ = new CpModelProto();
    constant_map_ = new Dictionary<long, IntVar>();
  }

  // Getters.

  public CpModelProto Model
  {
    get { return model_; }
  }

  int Negated(int index)
  {
    return -index - 1;
  }

  // Integer variables and constraints.

  public IntVar NewIntVar(long lb, long ub, string name)
  {
    long[] bounds = { lb, ub };
    return new IntVar(model_, bounds, name);
  }

  public IntVar NewIntVar(IEnumerable<long> bounds, string name)
  {
    return new IntVar(model_, bounds, name);
  }

  // TODO: Add optional version of above 2 NewIntVar().

  public IntVar NewBoolVar(string name)
  {
    long[] bounds = { 0L, 1L };
    return new IntVar(model_, bounds, name);
  }

  public Constraint AddLinearConstraint(IEnumerable<Tuple<IntVar, long>> terms,
                                        long lb, long ub)
  {
    Constraint ct = new Constraint(model_);
    ConstraintProto model_ct = ct.Proto();
    foreach (Tuple<IntVar, long> term in terms)
    {
      model_ct.Linear.Vars.Add(term.Item1.Index);
      model_ct.Linear.Coeffs.Add(term.Item2);
    }
    model_ct.Linear.Domain.Add(lb);
    model_ct.Linear.Domain.Add(ub);
    return ct;
  }

  public Constraint AddSumConstraint(IEnumerable<IntVar> vars, long lb,
                                     long ub)
  {
    Constraint ct = new Constraint(model_);
    ConstraintProto model_ct = ct.Proto();
    foreach (IntVar var in vars)
    {
      model_ct.Linear.Vars.Add(var.Index);
      model_ct.Linear.Coeffs.Add(1L);
    }
    model_ct.Linear.Domain.Add(lb);
    model_ct.Linear.Domain.Add(ub);
    return ct;
  }

  // TODO: AddLinearConstraintWithBounds

  public Constraint Add(BoundIntegerExpression lin)
  {
    // TODO: Implement me.
    return null;
  }


  public Constraint AddAllDifferent(IEnumerable<IntVar> vars)
  {
    Constraint ct = new Constraint(model_);
    ConstraintProto model_ct = ct.Proto();
    foreach (IntVar var in vars)
    {
      model_ct.AllDiff.Vars.Add(var.Index);
    }
    return ct;
  }

  // TODO: AddElement

  // TODO: AddCircuit

  // TODO: AddAllowedAssignments

  // TODO: AddForbiddenAssignments

  // TODO: AddAutomata

  // TODO: AddInverse

  // TODO: AddReservoirConstraint

  // TODO: AddMapDomain

  // TODO: AddImplication

  // TODO: AddBoolOr

  // TODO: AddBoolAnd

  // TODO: AddBoolXOr

  // TODO: AddMinEquality

  // TODO: AddMaxEquality

  // TODO: AddDivisionEquality

  // TODO: AddModuloEquality

  // TODO: AddProdEquality

  // Scheduling support

  // TODO: NewInterval

  // Objective.
  public void Minimize(IntegerExpression obj)
  {
    SetObjective(obj, true);
  }

  public void Maximize(IntegerExpression obj)
  {
    SetObjective(obj, false);
  }

  bool HasObjective()
  {
    return model_.Objective == null;
  }

  // Internal methods.

  void SetObjective(IntegerExpression obj, bool minimize)
  {
    CpObjectiveProto objective = new CpObjectiveProto();
    if (obj is IntVar)
    {
      objective.Coeffs.Add(1L);
      objective.Offset = 0L;
      if (minimize)
      {
        objective.Vars.Add(obj.Index);
        objective.ScalingFactor = 1L;
      }
      else
      {
        objective.Vars.Add(Negated(obj.Index));
        objective.ScalingFactor = -1L;
      }
    }
    model_.Objective = objective;
    // TODO: Implement me for general IntegerExpression.
  }

  private CpModelProto model_;
  private Dictionary<long, IntVar> constant_map_;
}

public class CpSolver
{
  public CpSolver()
  {
  }

  public CpSolverStatus Solve(CpModel model)
  {
    if (string_parameters_ != null)
    {
      response_ = SatHelper.SolveWithStringParameters(model.Model,
                                                      string_parameters_);
    }
    else
    {
      response_ = SatHelper.Solve(model.Model);
    }
    return response_.Status;
  }

  public string StringParameters
  {
    get { return string_parameters_; }
    set { string_parameters_ = value; }
  }

  public CpSolverResponse Response
  {
    get { return response_; }
  }

  private CpModelProto model_;
  private CpSolverResponse response_;
  string string_parameters_;
}

}  // namespace Google.OrTools.Sat