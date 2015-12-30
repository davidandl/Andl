﻿/// Andl is A New Data Language. See andl.org.
///
/// Copyright © David M. Bennett 2015 as an unpublished work. All rights reserved.
///
/// If you have received this file directly from me then you are hereby granted 
/// permission to use it for personal study. For any other use you must ask my 
/// permission. Not to be copied, distributed or used commercially without my 
/// explicit written permission.
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Andl.Runtime;

namespace Andl.Peg {
  /// <summary>
  /// Implements meaning for the symbol
  /// </summary>
  public enum SymKinds {
    NUL,
    ALIAS,      // alias
    CONST,      // value known at compile time
    FIELD,      // value during tuple iteration
    CATVAR,     // value as catalog variable
    PARAM,      // value from local declaration
    FUNC,       // callable function with args
    SELECTOR,   // for UDT
    COMPONENT,  // for UDT
  }

  /// <summary>
  /// Define how this function should be called
  /// </summary>
  public enum CallKinds {
    NUL,      // Not
    FUNC,     // Simple function
    VFUNC,    // Function with variable args (CodeValue)
    VFUNCT,   // Ditto, TypedValue
    JFUNC,    // Dyadic join-like
    EFUNC,    // Expression code
    LFUNC,    // tuple lookup function
    UFUNC,    // UDT lookup function
    SFUNC,    // SELECTOR for udt
    CFUNC,    // compile-time function
  };

  /// <summary>
  /// Join operation implemented by this function
  /// Note: LCR must be numerically same as MergeOps
  /// </summary>
  [Flags]
  public enum JoinOps {
    // basic values
    NUL, LEFT = 1, COMMON = 2, RIGHT = 4, 
    SETL = 8, SETC = 16, SETR = 32, 
    ANTI = 64, SET = 128, REV = 256, ERROR = 512,
    // mask combos
    MERGEOPS = LEFT | COMMON | RIGHT,
    SETOPS = SETL | SETC | SETR,
    OTHEROPS = ANTI | REV | ERROR,
    // joins
    JOIN = LEFT | COMMON | RIGHT, 
    COMPOSE = LEFT | RIGHT,
    DIVIDE = LEFT,
    RDIVIDE = RIGHT,
    SEMIJOIN = LEFT | COMMON, 
    RSEMIJOIN = RIGHT | COMMON,
    // antijoins
    ANTIJOIN = ANTI | LEFT | COMMON,
    ANTIJOINL = ANTI | LEFT,
    RANTIJOIN = ANTI | RIGHT | COMMON | REV,
    RANTIJOINR = ANTI | RIGHT | REV, 
    // set
    UNION = SET | COMMON | SETL | SETC | SETR,
    INTERSECT = SET | COMMON | SETC,
    SYMDIFF = SET | COMMON | SETL | SETR,
    MINUS = SET | COMMON | SETL,
    RMINUS = SET | COMMON | SETR | REV,
  };

  // Defines a foldable function
  public enum FoldableFlags {
    NUL, ANY, GROUP, ORDER
  };

  // Defines a fold function seed
  public enum FoldSeeds {
    NUL, ZERO, ONE, MIN, MAX, FALSE, TRUE
  }

  // internal names for builtins
  public class SymNames {
    public const string Assign = ":assign";
    public const string Defer = ":defer";
    public const string DoBlock = ":doblock";
    public const string Invoke = ":invoke";
    public const string Lift = ":lift";
    public const string Project = ":project";
    public const string Rename = ":rename";
    public const string Restrict = ":restrict";
    public const string Row = ":row";
    public const string Table = ":table";
    public const string Transform = ":transform";
    public const string TransAgg = ":transagg";
    public const string TransOrd = ":transord";
    public const string UpdateJoin = ":upjoin";
    public const string UpdateTransform = ":uptransform";
    public const string UserSelector = ":userselector";
  }

  /// <summary>
  /// Implements a symbol table entry.
  /// </summary>
  public class Symbol {
    public SymKinds Kind { get; set; }
    public DataType DataType { get; set; }
    public DataHeading Heading { get {
      return DataType is DataTypeRelation ? (DataType as DataTypeRelation).Heading
        : DataType is DataTypeTuple ? (DataType as DataTypeRelation).Heading
        : null; // FIX: symbol heading
    } }
    public string Name { get; set; }
    public int Precedence { get; set; }
    public int NumArgs { get; set; }
    public TypedValue Value { get; set; }
    public CallInfo CallInfo { get; set; }
    public TypedValue Seed { get; set; }
    public FoldSeeds FoldSeed { get; set; }
    public JoinOps JoinOp { get; set; }
    public CallKinds CallKind { get; set; }
    public FoldableFlags Foldable { get; set; }
    public Symbol Link { get; set; }
    public int Level { get; set; }

    public MergeOps MergeOp { get { return (MergeOps)(JoinOp & JoinOps.MERGEOPS); } }
    
    public const string Assign = ":assign";
    public const string Defer = ":defer";
    public const string DoBlock = ":doblock";
    public const string Invoke = ":invoke";
    public const string Lift = ":lift";
    public const string Project = ":project";
    public const string Rename = ":rename"; 
    public const string Restrict = ":restrict";
    public const string Row = ":row";
    public const string Table = ":table";
    public const string Transform = ":transform";
    public const string TransAgg = ":transagg";
    public const string TransOrd = ":transord";
    public const string UpdateJoin = ":upjoin";
    public const string UpdateTransform = ":uptransform";
    public const string UserSelector = ":userselector";
    
    public override string ToString() {
      return String.Format("{0}:{1}:{2}", Name, Kind, Level);
    }

    // series of tests used by parser
    //public bool Is(Atoms atom) { return Atom == atom; }
    //public bool IsLiteral { get { return Atom == Atoms.LITERAL; } }
    //public bool IsIdent { get { return Atom == Atoms.IDENT; } }
    //public bool IsDefinable { get { return Atom == Atoms.IDENT && Level != Scope.Current.Level; } }
    //public bool IsUndefIdent { get { return Atom == Atoms.IDENT && Kind == SymKinds.UNDEF; } }
    //public bool IsField { get { return Kind == SymKinds.FIELD; } }
    //public bool IsLookup { get { return Kind == SymKinds.FIELD || Kind == SymKinds.PARAM; } }
    //public bool IsFoldable { get { return Foldable != FoldableFlags.NUL; } }
    //public bool IsDyadic { get { return MergeOp != MergeOps.Nul; } }
    //public bool IsUnary { get { return Kind == SymKinds.UNOP || Atom == Atoms.MINUS; } }
    //public bool IsBinary { get { return Kind == SymKinds.BINOP; } }
    //public bool IsOperator { get { return IsBinary || IsUnary; } }
    //public bool IsFunction { get { return CallKind != CallKinds.NUL && !IsOperator; } }
    //public bool IsBuiltIn { get { return CallInfo != null; } }
    //public bool IsCompareOp { get { return IsBinary && DataType == DataTypes.Bool && !IsFoldable; } }
    //public bool IsGlobal { get { return Level == 1; } }

    public bool IsConst { get { return Kind == SymKinds.CONST; } }
    public bool IsCatVar { get { return Kind == SymKinds.CATVAR; } }
    public bool IsField { get { return Kind == SymKinds.FIELD; } }
    public bool IsParam { get { return Kind == SymKinds.PARAM; } }
    // Variable means a name bound to a value
    public bool IsVariable { get { return IsConst || IsCatVar || IsField || IsParam; } }

    public bool IsComponent { get { return Kind == SymKinds.COMPONENT; } }
    public bool IsUserType { get { return Kind == SymKinds.SELECTOR; } }

    public bool IsDefFunc { get { return CallKind == CallKinds.EFUNC; } }
    public bool IsCallable { get { return CallKind != CallKinds.NUL; } }
    public bool IsOperator { get { return IsCallable && Precedence != 0; } }
    public bool IsFoldable { get { return IsCallable && Foldable != FoldableFlags.NUL; } }
    public bool IsDyadic { get { return IsCallable && MergeOp != MergeOps.Nul; } }
    public bool IsUnary { get { return IsOperator && NumArgs == 1; } }
    public bool IsBinary { get { return IsOperator && NumArgs == 2; } }
    public bool IsCompareOp { get { return IsBinary && DataType == DataTypes.Bool && !IsFoldable; } }

    public DataType ReturnType { get { return CallInfo.ReturnType; } }

    public TypedValue GetSeed(DataType datatype) {
      if (datatype is DataTypeRelation)
        return RelationValue.Create(DataTable.Create(datatype.Heading));

      switch (FoldSeed) {
      case FoldSeeds.NUL:
        if (datatype == DataTypes.Bool) return BoolValue.False;
        if (datatype == DataTypes.Text) return TextValue.Default;
        if (datatype == DataTypes.Number) return NumberValue.Zero;
        if (datatype == DataTypes.Time) return TimeValue.Minimum;
        break;
      case FoldSeeds.ZERO:
        if (datatype == DataTypes.Number) return NumberValue.Zero;
        break;
      case FoldSeeds.ONE:
        if (datatype == DataTypes.Number) return NumberValue.One;
        break;
      case FoldSeeds.MIN:
        if (datatype == DataTypes.Number) return NumberValue.Minimum;
        if (datatype == DataTypes.Time) return TimeValue.Minimum;
        break;
      case FoldSeeds.MAX:
        if (datatype == DataTypes.Number) return NumberValue.Maximum;
        if (datatype == DataTypes.Time) return TimeValue.Maximum;
        break;
      case FoldSeeds.FALSE:
        if (datatype == DataTypes.Bool) return BoolValue.False;
        break;
      case FoldSeeds.TRUE:
        if (datatype == DataTypes.Bool) return BoolValue.True;
        break;
      default:
        break;
      }
      return null;
    }
  }

  ///-------------------------------------------------------------------

  /// <summary>
  /// SymbolTable implements the main compiler symbol table.
  /// </summary>
  public class SymbolTable {
    Scope _importscope;
    Catalog _catalog;
    HashSet<string> _sources = new HashSet<string>();

    //public static SymbolTable Create() {
    public static SymbolTable Create(Catalog catalog) {
      var st = new SymbolTable { _catalog = catalog };
      st.Init();
      return st;
    }

    //--- publics

    // Add a symbol to the catalog, but only if it is global
    public void AddCatalog(Symbol symbol) {
      if (Scope.Current.IsGlobal) {
        var kind = symbol.IsUserType ? EntryKinds.Type
          : symbol.IsDefFunc ? EntryKinds.Code
          : EntryKinds.Value;
        var flags = EntryFlags.Public;  // FIX: when visibility control implemented
        _catalog.GlobalVars.AddNew(symbol.Name, symbol.DataType, kind, flags);
      }
    }

    // Find existing symbol by name
    public Symbol FindIdent(string name) {
      var sym = Scope.Current.FindAny(name);
      return (sym != null && sym.Kind == SymKinds.ALIAS) ? sym.Link : sym;
    }

    public bool IsDefinable(string name) {
      var sym = Scope.Current.FindAny(name);
      return sym == null || sym.Level != Scope.Current.Level;
    }

    // Find existing source by name
    public bool IsSource(string name) {
      return _sources.Contains(name);
    }

    public void AddUserType(string name, DataTypeUser datatype) {
      Scope.Current.Add(MakeUserType(name, datatype));
    }

    public void AddVariable(string name, DataType datatype, SymKinds kind) {
      Scope.Current.Add(MakeVariable(name, datatype, kind));
    }

    public void AddDeferred(string name, DataType rettype, DataColumn[] args) {
      Scope.Current.Add(MakeDeferred(name, rettype, args));
    }


    ///-------------------------------------------------------------------

    // Make symbols that can be user-defined
    internal static Symbol MakeUserType(string name, DataTypeUser datatype) {
      var callinfo = CallInfo.Create(name, datatype, datatype.Heading.Columns.ToArray());
      return new Symbol {
        Name = name,
        Kind = SymKinds.SELECTOR,
        CallKind = CallKinds.SFUNC,
        NumArgs = callinfo.NumArgs,
        DataType = datatype,
        CallInfo = callinfo,
      };
    }

    public static Symbol MakeVariable(string name, DataType datatype, SymKinds kind) {
      Symbol sym = new Symbol {
        Name = name,
        Kind = kind,
        DataType = datatype,
      };
      return sym;
    }

    public static Symbol MakeDeferred(string name, DataType datatype, DataColumn[] args) {
      Symbol sym = new Symbol {
        Name = name,
        Kind = SymKinds.CATVAR,
        DataType = datatype,
        CallKind = CallKinds.EFUNC,
        CallInfo = CallInfo.Create(name, datatype, args),
        NumArgs = args.Length,
      };
      return sym;
    }

    //--- setup

    void Init() {
      Scope.Push();
      AddSymbols();
      foreach (var info in AddinInfo.GetAddinInfo())
        AddBuiltinFunction(info.Name, info.NumArgs, info.DataType, info.Method);
      _importscope = Scope.Push();  // reserve a level for imported symbols
      Scope.Current.IsGlobal = true;
    }

    // Process catalog to add all entries from persistent level
    // Called functions should discard duplicates, or flag errors???
    public void Import(CatalogScope catalogscope) {
      foreach (var entry in catalogscope.GetEntries()) {
        var value = entry.Value;
        if (_importscope.Find(entry.Name) == null)
          Logger.WriteLine(3, "From catalog add {0}:{1}", entry.Name, entry.DataType.BaseType.Name);

        if (entry.Kind == EntryKinds.Type)
          _importscope.Add(MakeUserType(entry.Name, entry.DataType as DataTypeUser));
        else if (entry.Kind == EntryKinds.Value)
          _importscope.Add(MakeVariable(entry.Name, entry.DataType, SymKinds.CATVAR));
        else if (entry.Kind == EntryKinds.Code)
          _importscope.Add(MakeDeferred(entry.Name, entry.DataType, entry.CodeValue.Value.Lookup.Columns));
      }
    }

    // Add a built in function (from a library)
    Symbol AddBuiltinFunction(string name, int numargs, DataType type, string method) {
      return AddFunction(name, numargs, type, CallKinds.FUNC, method);
    }

    //------------------------------------------------------------------
    //-- ops

    //static Symbol MakeLiteral(TypedValue value) {
    //  Symbol sym = new Symbol {
    //    Kind = SymKinds.LITERAL,
    //    DataType = value.DataType,
    //    Value = value
    //  };
    //  return sym;
    //}

    //static internal Symbol MakeIdent(string name = null) {
    //  Symbol sym = new Symbol {
    //    Name = name,
    //    Kind = SymKinds.UNDEF,
    //    DataType = DataTypes.Unknown,
    //  };
    //  return sym;
    //}

    // Load and initialise the symbol table
    void AddSymbols() {
      AddIdent("true", SymKinds.CONST, BoolValue.True, DataTypes.Bool);
      AddIdent("false", SymKinds.CONST, BoolValue.False, DataTypes.Bool);
      AddIdent("$lineno$", SymKinds.CONST, NumberValue.Zero, DataTypes.Number);
      AddIdent("$filename$", SymKinds.CONST, TextValue.Empty, DataTypes.Text);

      AddOperator("not", 1, 9, DataTypes.Bool, "Not");
      AddOperator("**", 2, 9, DataTypes.Number, "Pow");
      AddOperator("u-", 1, 8, DataTypes.Number, "Neg");
      AddOperator("*", 2, 7, DataTypes.Number, "Multiply", FoldableFlags.ANY, FoldSeeds.ONE);
      AddOperator("/", 2, 7, DataTypes.Number, "Divide", FoldableFlags.ORDER, FoldSeeds.ONE);
      AddOperator("div", 2, 7, DataTypes.Number, "Div");
      AddOperator("mod", 2, 7, DataTypes.Number, "Mod");
      AddOperator("+", 2, 6, DataTypes.Number, "Add", FoldableFlags.ANY, FoldSeeds.ZERO);
      AddOperator("-", 2, 6, DataTypes.Number, "Subtract", FoldableFlags.ORDER, FoldSeeds.ZERO);
      AddOperator("&", 2, 5, DataTypes.Text, "Concat", FoldableFlags.ORDER, FoldSeeds.NUL);

      AddOperator("=", 2, 4, DataTypes.Bool, "Eq");
      AddOperator("<>", 2, 4, DataTypes.Bool, "Ne");
      AddOperator(">", 2, 4, DataTypes.Bool, "Gt");
      AddOperator(">=", 2, 4, DataTypes.Bool, "Ge");
      AddOperator("<", 2, 4, DataTypes.Bool, "Lt");
      AddOperator("<=", 2, 4, DataTypes.Bool, "Le");
      AddOperator("=~", 2, 4, DataTypes.Bool, "Match");

      // These are overloaded by bit operations on integers
      AddOperator("and", 2, 3, DataTypes.Unknown, "And,BitAnd", FoldableFlags.ANY, FoldSeeds.TRUE);
      AddOperator("or", 2, 2, DataTypes.Unknown, "Or,BitOr", FoldableFlags.ANY, FoldSeeds.FALSE);
      AddOperator("xor", 2, 2, DataTypes.Unknown, "Xor,BitXor", FoldableFlags.ANY, FoldSeeds.FALSE);

      AddOperator("sub", 2, 4, DataTypes.Bool, "Subset");
      AddOperator("sup", 2, 4, DataTypes.Bool, "Superset");
      AddOperator("sep", 2, 4, DataTypes.Bool, "Separate");

      AddFunction(SymNames.Assign, 1, DataTypes.Void, CallKinds.FUNC, "Assign");
      AddFunction(SymNames.Defer, 1, DataTypes.Void, CallKinds.FUNC, "Defer");
      AddFunction(SymNames.DoBlock, 1, DataTypes.Any, CallKinds.FUNC, "DoBlock");
      AddFunction(SymNames.Invoke, 2, DataTypes.Any, CallKinds.VFUNCT, "Invoke");
      AddFunction(SymNames.Lift, 1, DataTypes.Void, CallKinds.FUNC, "Lift");
      AddFunction(SymNames.Project, 2, DataTypes.Table, CallKinds.VFUNC, "Project");
      AddFunction(SymNames.Rename, 2, DataTypes.Table, CallKinds.VFUNC, "Rename");
      AddFunction(SymNames.Row, 2, DataTypes.Row, CallKinds.VFUNC, "Row");
      AddFunction(SymNames.Restrict, 2, DataTypes.Table, CallKinds.VFUNC, "Restrict");
      AddFunction(SymNames.Transform, 2, DataTypes.Table, CallKinds.VFUNC, "Transform");
      AddFunction(SymNames.TransAgg, 2, DataTypes.Table, CallKinds.VFUNC, "TransAgg");
      AddFunction(SymNames.TransOrd, 2, DataTypes.Table, CallKinds.VFUNC, "TransOrd");
      AddFunction(SymNames.Table, 2, DataTypes.Table, CallKinds.VFUNC, "Table");
      AddFunction(SymNames.UpdateJoin, 3, DataTypes.Bool, CallKinds.FUNC, "UpdateJoin");
      AddFunction(SymNames.UpdateTransform, 3, DataTypes.Bool, CallKinds.VFUNC, "UpdateTrans");
      AddFunction(SymNames.UserSelector, 2, DataTypes.User, CallKinds.VFUNCT, "UserSelector");

      AddFunction("max", 2, DataTypes.Ordered, CallKinds.FUNC, "Max", FoldableFlags.ANY, FoldSeeds.MIN);
      AddFunction("min", 2, DataTypes.Ordered, CallKinds.FUNC, "Min", FoldableFlags.ANY, FoldSeeds.MAX);
      AddFunction("fold", 0, DataTypes.Unknown, CallKinds.FUNC, "Fold");
      AddFunction("cfold", 2, DataTypes.Unknown, CallKinds.FUNC, "CumFold");
      AddFunction("if", 3, DataTypes.Unknown, CallKinds.FUNC, "If");
      AddFunction("recurse", 2, DataTypes.Unknown, CallKinds.FUNC, "Recurse");

      AddFunction("ord", 0, DataTypes.Number, CallKinds.LFUNC, "Ordinal");
      AddFunction("ordg", 0, DataTypes.Number, CallKinds.LFUNC, "OrdinalGroup");
      AddFunction("lead", 0, DataTypes.Unknown, CallKinds.LFUNC, "ValueLead");
      AddFunction("lag", 0, DataTypes.Unknown, CallKinds.LFUNC, "ValueLag");
      AddFunction("nth", 0, DataTypes.Unknown, CallKinds.LFUNC, "ValueNth");
      AddFunction("rank", 0, DataTypes.Unknown, CallKinds.LFUNC, "Rank");

      AddDyadic("join", 2, 4, JoinOps.JOIN, "DyadicJoin");
      AddDyadic("compose", 2, 4, JoinOps.COMPOSE, "DyadicJoin");
      AddDyadic("divide", 2, 4, JoinOps.DIVIDE, "DyadicJoin");
      AddDyadic("rdivide", 2, 4, JoinOps.RDIVIDE, "DyadicJoin");
      AddDyadic("semijoin", 2, 4, JoinOps.SEMIJOIN, "DyadicJoin");
      AddDyadic("rsemijoin", 2, 4, JoinOps.RSEMIJOIN, "DyadicJoin");

      AddDyadic("ajoin", 2, 4, JoinOps.ANTIJOIN, "DyadicAntijoin");
      AddDyadic("rajoin", 2, 4, JoinOps.RANTIJOIN, "DyadicAntijoin");
      AddDyadic("ajoinl", 2, 4, JoinOps.ANTIJOINL, "DyadicAntijoin");
      AddDyadic("rajoinr", 2, 4, JoinOps.RANTIJOINR, "DyadicAntijoin");

      AddDyadic("union", 2, 4, JoinOps.UNION, "DyadicSet");
      AddDyadic("intersect", 2, 4, JoinOps.INTERSECT, "DyadicSet");
      AddDyadic("symdiff", 2, 4, JoinOps.SYMDIFF, "DyadicSet");
      AddDyadic("minus", 2, 4, JoinOps.MINUS, "DyadicSet");
      AddDyadic("rminus", 2, 4, JoinOps.RMINUS, "DyadicSet");

      AddAlias("matching", "semijoin");
      AddAlias("notmatching", "ajoin");
      AddAlias("joinlr", "compose");
      AddAlias("joinlc", "matching");
      AddAlias("joinl", "divide");
      AddAlias("joincr", "rsemijoin");
      AddAlias("joinr", "rdivide");

      AddSource("csv");
      AddSource("txt");
      AddSource("sql");
      AddSource("con");
      AddSource("file");
      AddSource("oledb");
      AddSource("odbc");
    }

    // Add a symbol to the current scope
    Symbol Add(string name, Symbol sym) {
      Scope.Current.Add(sym, name);
      return sym;
    }

    Symbol AddIdent(string name, SymKinds kind, TypedValue value, DataType type) {
      return Add(name, new Symbol {
        Kind = kind,
        DataType = type,
        Value = value
      });
    }

    Symbol AddOperator(string name, int numargs, int precedence, DataType type, string method, FoldableFlags foldable = FoldableFlags.NUL, FoldSeeds seed = FoldSeeds.NUL) {
      return Add(name, new Symbol {
        Kind = SymKinds.FUNC,
        CallKind = CallKinds.FUNC,
        NumArgs = numargs,
        Precedence = precedence,
        DataType = type,
        Foldable = foldable,
        FoldSeed = seed,
        CallInfo = CallInfo.Get(method),
      });
    }

    Symbol AddDyadic(string name, int numargs, int precedence, JoinOps joinop, string method) {
      return Add(name, new Symbol {
        Kind = SymKinds.FUNC,
        CallKind = CallKinds.JFUNC,
        NumArgs = numargs,
        Precedence = precedence,
        JoinOp = joinop,
        DataType = DataTypes.Unknown,
        CallInfo = CallInfo.Get(method),
        Foldable = (joinop.HasFlag(JoinOps.LEFT) == joinop.HasFlag(JoinOps.RIGHT)) ? FoldableFlags.ANY : FoldableFlags.NUL,
      });
    }

    Symbol AddFunction(string name, int numargs, DataType type, CallKinds callkind, string method, FoldableFlags foldable = FoldableFlags.NUL, FoldSeeds seed = FoldSeeds.NUL) {
      return Add(name, new Symbol {
        Kind = SymKinds.FUNC,
        CallKind = callkind,
        NumArgs = numargs,
        DataType = type,
        Foldable = foldable,
        FoldSeed = seed,
        CallInfo = CallInfo.Get(method),
      });
    }

    Symbol AddAlias(string name, string other) {
      return Add(name, new Symbol {
        Kind = SymKinds.ALIAS,
        Link = FindIdent(other),
      });
    }

    void AddSource(string name) {
      _sources.Add(name);
    }

  }
}
