using System.Linq.Expressions;

public abstract class Statement : ASTNode
{
    public Statement(int line, int column) : base(line, column){}
}


public class AssigmentNode : Statement
{
    public string VariableName { get; }
    public ExpressionNode Value {get; }

    public AssigmentNode (string variableName, ExpressionNode value, int line, int column): base(line, column)
    {
        VariableName = variableName;
        Value = value;
    }
}

public class LabelNode : Statement
{
    public string Name {get; }
    public ProgramNode Programa {get; }

    public LabelNode(string name, ProgramNode programa, int line, int column) : base(line, column)
    {
        Name = name;
        Programa = programa;
    }
}

public class Instruction : Statement
{
    public Tipo Name {get; }
    public List<Expression> Arguments {get; }

    public Instruction(Tipo name, List<Expression> arguments, int line, int column) : base(line, column)
    {
        Name = name;
        Arguments = arguments;
    }
}
