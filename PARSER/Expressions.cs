using System.Linq.Expressions;

public abstract class ExpressionNode : ASTNode
{
    public ExpressionNode(int line, int column) : base(line, column) {}

    
}

public abstract class ArithmeticExpression : ExpressionNode
{
    public ArithmeticExpression(int line, int column) : base(line, column) {}
}

public class AdditiveExpression : ArithmeticExpression
{
    public MultiplicativeExpression Left {get; }
    public Tipo Operator {get; }
    public MultiplicativeExpression Right {get; }

    public AdditiveExpression(MultiplicativeExpression left, MultiplicativeExpression right, Tipo op, int line, int column) : base(line, column)
    {
        Left = left;
        Right = right;
        Operator = op;
    }
}

public class MultiplicativeExpression : ArithmeticExpression
{
    public PowerExpression Left {get; }
    public Tipo Operator {get; }
    public PowerExpression Right {get; }

    public MultiplicativeExpression(PowerExpression left, PowerExpression right, Tipo op, int line, int column) : base(line, column)
    {
        Left = left;
        Right = right;
        Operator = op;
    }
}

public class PowerExpression : ArithmeticExpression
{
    public UnaryExpressionNode Base {get; }
    
    public UnaryExpressionNode Exponent {get; }

    public PowerExpression(UnaryExpressionNode base_, UnaryExpressionNode exponent,  int line, int column) : base(line, column)
    {
        Base = base_;
        Exponent = exponent;
    }

    public static implicit operator PowerExpression(MultiplicativeExpression v)
    {
        throw new NotImplementedException();
    }
}

public class UnaryExpressionNode : ArithmeticExpression
{
    public bool  Sign {get; }
    public PrimaryExpression Module {get; }

    public UnaryExpressionNode(bool sign, PrimaryExpression module, int line, int column) : base(line, column)
    {
        Sign = sign;
        Module = module;

    }
}

public abstract class PrimaryExpression : ArithmeticExpression
{
    public PrimaryExpression(int line, int column) : base(line, column) {}
}
public class NumberNode: PrimaryExpression
{
    public int Value {get; }
    public NumberNode (int value, int line, int column) : base(line, column)
    {
        Value = value;
    }
}

public class VariableNode : PrimaryExpression
{
    public string Name {get; }

    public VariableNode(string name, int line, int column) : base(line, column)
    {
        Name = name;
    }
}

public class FunctionCall : PrimaryExpression
{
    public Tipo Name {get; }
    public List<Expression> Arguments {get; }

    public FunctionCall(Tipo name, List<Expression> arguments, int line, int column) : base(line, column)
    {
        Name = name;
        Arguments = arguments;
    }
}

public abstract class BooleanExpression : ExpressionNode
{
    public BooleanExpression(int line, int column) : base(line, column) {}
}

public class OrExpression : BooleanExpression
{
    public AndExpression Left {get; }
    public AndExpression Right {get; }

    public OrExpression(AndExpression left, AndExpression right, int line, int column ) : base(line, column)
    {
        Left = left;
        Right = right;
    }
}

public class AndExpression : BooleanExpression
{
    public ComparisonExpression Left {get; }
    public ComparisonExpression Right {get; }

    public AndExpression(ComparisonExpression left, ComparisonExpression right, int line, int column ) : base(line, column)
    {
        Left = left;
        Right = right;
    }
}

public class ComparisonExpression : BooleanExpression
{
    public ArithmeticExpression Left {get; }
    public Tipo Operator {get; }
    public ArithmeticExpression Right {get; }

    public ComparisonExpression(ArithmeticExpression left,Tipo op, ArithmeticExpression right, int line, int column ) : base(line, column)
    {
        Left = left;
        Right = right;
        Operator = op;
    }
}

public class StringExpression :ExpressionNode
{
    public string Value {get; }

    public StringExpression(string value, int line, int column) : base(line, column)
    {
        Value = value;
    }
}



