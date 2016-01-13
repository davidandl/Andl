﻿using System;
using System.Linq;
using Andl.Runtime;

namespace Andl.Peg {
  /// <summary>
  /// Base class for AST nodes
  /// </summary>
  public class AstNode {
    public DataType DataType { get; set; }
    public virtual void Emit(Emitter e) { }

    // utility function to create a code segment for nested calls
    public ByteCode Compile() {
      var e = new Emitter();
      Emit(e);
      return e.GetCode();
    }

    // create code segment wrapped for fold
    public ByteCode Compile(Symbol op, TypedValue seed) {
      var e = new Emitter();
      e.Out(Opcodes.LDAGG, seed);
      Emit(e);
      e.OutCall(op);
      return e.GetCode();
    }
  }

  public class AstType : AstNode { }
  // A statement also can be a define or a value/expression
  public class AstStatement : AstNode { }
  public class AstDefine : AstStatement { }
  public class AstUserType : AstDefine { }
  public class AstSubType : AstDefine { }

  /// <summary>
  /// Base class for AST fields used in transform
  /// </summary>
  public class AstField : AstNode {
    public string Name { get; set; }
    //public DataType DataType { get; set; }
  }

  public class AstProject : AstField {
    public override string ToString() {
      return string.Format("{0}", Name);
    }
    public override void Emit(Emitter e) {
      e.OutSeg(ExpressionBlock.Create(Name, Name, DataType));
    }
  }

  public class AstRename : AstField {
    public string OldName { get; set; }
    public override string ToString() {
      return string.Format("{0}:={1}", Name, OldName);
    }
    public override void Emit(Emitter e) {
      e.OutSeg(ExpressionBlock.Create(Name, OldName, DataType));
    }
  }

  public class AstExtend : AstField {
    public AstValue Value { get; set; }
    public DataHeading Lookup { get; set; }
    public int Accums { get; set; }
    public override string ToString() {
      return string.Format("{0}:={1}", Name, Value);
    }
    public override void Emit(Emitter e) {
      var kind = (Accums > 0) ? ExpressionKinds.HasFold : ExpressionKinds.Open;
      e.OutSeg(ExpressionBlock.Create(Name, kind, Value.Compile(), Value.DataType, Accums, Lookup));
    }
  }

  public class AstLift : AstExtend {
    public override string ToString() {
      return string.Format("Lift({0})", Value);
    }
  }

  public class AstOrder : AstField {
    public bool Descending { get; set; }
    public bool Grouped { get; set; }
    public override string ToString() {
      return string.Format("{0}{1}{2}", Descending ? "-" :"", Grouped ? "%" : "", Name);
    }
    public override void Emit(Emitter e) {
      e.OutSeg(ExpressionBlock.Create(Name, DataType, Grouped, Descending));
    }
  }

  /// <summary>
  /// Base class for AST values
  /// </summary>
  public class AstValue : AstStatement { } // FIX: probably wrong

  public class AstVariable : AstValue {
    public Symbol Variable { get; set; }
    public override string ToString() {
      return string.Format("{0}:{1}", Variable.Name, DataType);
    }
    public override void Emit(Emitter e) {
      e.OutLoad(Variable);
    }
  }

  public class AstLiteral : AstValue {
    public TypedValue Value { get; set; }
    public override string ToString() {
      return Value.ToString();
    }
    public override void Emit(Emitter e) {
      e.OutLoad(Value);
    }
  }

  public class AstBlock : AstValue {
    public AstStatement[] Statements { get; set; }
    public override string ToString() {
      return string.Format("[{0}]", String.Join("; ", Statements.Select(s => s.ToString())));
    }
    public override void Emit(Emitter e) {
      foreach (var s in Statements) s.Emit(e);
    }
  }

  public class AstDoBlock : AstValue {
    public Symbol Func { get; set; }
    public AstValue Value { get; set; }
    public override void Emit(Emitter e) {
      e.OutSeg(ExpressionBlock.Create(":d", ExpressionKinds.Closed, Value.Compile(), Value.DataType));
      e.Out(Opcodes.LDACCBLK);
      e.OutCall(Func);
    }
  }

  public class AstBodyStatement : AstValue {
    public AstStatement Statement { get; set; }
    public int AccumCount { get; set; }
  }

  public class AstCode : AstValue {
    public string Name { get; set; }
    public AstValue Value { get; set; }
    public DataHeading Lookup { get; set; }
    public int Accums { get; set; }
    public bool Defer { get; set; }   // output as code value not seg
    public override string ToString() {
      return string.Format("{0}({1})[{2}] => {3}", Name, Lookup, Value, Value.DataType);
    }
    public override void Emit(Emitter e) {
      var kind = (Accums > 0) ? ExpressionKinds.HasFold
        : (Lookup == null) ? ExpressionKinds.Closed : ExpressionKinds.Open;
      var eb = ExpressionBlock.Create(Name, kind, Value.Compile(), Value.DataType, Accums, Lookup);
      if (Defer) e.OutLoad(CodeValue.Create(eb));
      else e.OutSeg(eb);
    }
  }
  public class AstCall : AstValue {
    public Symbol Func { get; set; }
    public CallInfo CallInfo { get; set; }
  }
  public class AstFunCall : AstCall {
    public AstNode[] Arguments { get; set; }
    public int NumVarArgs { get; set; }

    public override string ToString() {
      return string.Format("{0}({1}) => {2}", Func.Name, String.Join(", ", Arguments.Select(a => a.ToString())), DataType);
    }
    public override void Emit(Emitter e) {
      Logger.Assert(DataType.IsVariable || DataType == DataTypes.Void, DataType);
      foreach (var a in Arguments) a.Emit(e);
      e.OutCall(Func, NumVarArgs, CallInfo);
    }
  }

  public class AstDefCall : AstFunCall {
    public Symbol DefFunc { get; set; }
    public int AccumBase { get; set; }
    public override void Emit(Emitter e) {
      Logger.Assert(DataType.IsVariable || DataType == DataTypes.Void, DataType);
      e.OutName(Opcodes.LDCATR, DefFunc);
      e.Out(Opcodes.LDACCBLK);
      e.OutLoad(NumberValue.Create(AccumBase));
      foreach (var a in Arguments) a.Emit(e);
      e.OutCall(Func, NumVarArgs);
    }
  }

  // Emits a call to fold()
  public class AstFoldCall : AstCall {
    public Symbol FoldedOp { get; set; }
    public AstValue FoldedExpr { get; set; }
    public int AccumIndex { get; set; }
    public override string ToString() {
      return string.Format("{0}({1},{2})#[{3}] => {4}", Func.Name, FoldedOp.Name, FoldedExpr, AccumIndex , DataType);
    }

    public override void Emit(Emitter e) {
      Logger.Assert(DataType.IsVariable || DataType == DataTypes.Void, DataType);

      e.Out(Opcodes.LDACCBLK);
      e.OutLoad(NumberValue.Create(AccumIndex));
      e.OutLoad(DataType.DefaultValue());
      var seed = FoldedOp.GetSeed(DataType);
      var eb = ExpressionBlock.Create(":i", ExpressionKinds.IsFolded, 
        FoldedExpr.Compile(FoldedOp, seed), FoldedExpr.DataType, AccumIndex);
      e.OutSeg(eb);
      e.OutCall(Func);
    }
  }

  public class AstTabCall : AstFunCall {
    public override void Emit(Emitter e) {
      Logger.Assert(DataType is DataTypeRelation || DataType is DataTypeTuple);

      e.OutLoad(HeadingValue.Create(DataType.Heading));
      foreach (var a in Arguments) a.Emit(e);
      e.OutCall(Func, Arguments.Length);
    }
  }

  public class AstUserCall : AstFunCall {
    public Symbol UserFunc { get; set; }
    public override void Emit(Emitter e) {
      Logger.Assert(DataType.IsVariable || DataType == DataTypes.Void, DataType);
      e.OutLoad(TextValue.Create(UserFunc.DataType.Name));
      foreach (var a in Arguments) a.Emit(e);
      e.OutCall(Func, NumVarArgs);
    }
  }

  public class AstTranCall : AstCall {
    public AstValue Where { get; set; }
    public AstOrderer Orderer { get; set; }
    public AstTransformer Transformer { get; set; }
  }

  public class AstOrderer : AstValue {
    public AstOrder[] Elements { get; set; }
    public override string ToString() {
      return string.Format("$({0})", String.Join(",", Elements.Select(e => e.ToString())));
    }
  }

  public class AstTransformer : AstValue {
    public bool Lift { get; set; }
    public AstField[] Elements { get; set; }
    public override string ToString() {
      return string.Format("{0}({1})", Lift ? "lift" : "tran", String.Join(",", Elements.Select(e => e.ToString())));
    }
  }

  public class AstComponent : AstFunCall {
    public override void Emit(Emitter e) {
      Logger.Assert(DataType.IsVariable || DataType == DataTypes.Void, DataType);
      Logger.Assert(Arguments.Length == 1 && Arguments[0].DataType is DataTypeUser);
      foreach (var a in Arguments) a.Emit(e);
      e.OutName(Opcodes.LDCOMP, Func);
    }
  }

  public class AstOpCall : AstFunCall { }
}
