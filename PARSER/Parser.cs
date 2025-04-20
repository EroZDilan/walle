public class Parser
{
    private List<Token> tokens;
    private int currentPosition;
    private Token currentToken;
    
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        this.currentPosition = 0;
        this.currentToken = tokens[0];
    }
    
    private void Advance()
    {
        currentPosition++;
        if (currentPosition < tokens.Count)
        {
            currentToken = tokens[currentPosition];
        }
    }
    
    private Token Consume(Tipo expectedType)
    {
        if (currentToken.Type == expectedType)
        {
            Token token = currentToken;
            Advance();
            return token;
        }
        throw new Exception($"Error de sintaxis: se esperaba {expectedType} pero se encontró {currentToken.Type} en línea {currentToken.Line}, columna {currentToken.Column}");
    }
    
    // Método principal para iniciar el parseo
    public ProgramNode Parse()
    {
        List<Statement> statements = new List<Statement>();
        
        while (currentToken.Type != Tipo.EOF)
        {
            statements.Add(ParseStatement());
        }
        
        return new ProgramNode(statements, tokens[0].Line, tokens[0].Column);
    }

    private Statement ParseStatement()
    {
        if(currentToken.Type == Tipo.IDENTIFIER) 
        {
            if(LookAhead(1) == Tipo.ASSIGN )return ParseAssigment();
            else
            {
                return ParseLabel();
            }
            
        }
        else if(currentToken.Type == Tipo.INSTRUCTION) return ParseInstruction();
        else
        {
            return ParseFunction();
        }
    }

    private Tipo LookAhead ()
    {
        int position = currentPosition + 1;
        if(position < tokens.Count)
        {
            return tokens[position].Type;
        }
        return Tipo.EOF;
    }

     private LabelNode ParseLabel()
    {
        string name = currentToken.Lexeme;
        Consume(Tipo.IDENTIFIER);
        
        // Parsea los statements que siguen a la etiqueta
        List<Statement> statements = new List<Statement>();
        while (currentToken.Type != Tipo.EOF )
        {
            statements.Add(ParseStatement());
        }
        
        return new LabelNode(name, new ProgramNode(statements, currentToken.Line, currentToken.Column),currentToken.Line, currentToken.Column);
    }

       private AssigmentNode ParseAssignment()
    {
        string name = currentToken.Lexeme;
        Consume(Tipo.IDENTIFIER);
        Consume(Tipo.ASSIGN); // Consume "<-"
        
        ExpressionNode expression = ParseExpression();
        
        return new AssigmentNode(name, expression, currentToken.Line, currentToken.Column);
    }

    private ExpressionNode ParseExpression()
    {
        if(currentToken.Type == Tipo.STRING) return ParseStringExpression();
        if(currentToken.Type == Tipo.BOOL) return ParseBooleanExpression();
        else
        {
            return ParseArithmeticExpression();
        }
    }

    private bool IsStartBoolean()
    {
        return currentToken.Type == Tipo.L_PAREN && IsBooleanOperator(LookAhead(2).Type);
    }

    private bool IsBooleanOperator(Tipo type)
    {
        return type == Tipo.EQUALS || 
               type == Tipo.GREATER || 
               type == Tipo.LESSER || 
               type == Tipo.GREATER_EQUAL || 
               type == Tipo.LESSER_EQUAL;
    }

    private ArithmeticExpression ParseArithmeticExpression()
    {
        return ParseAdditiveExpression();
    }
    
    private ArithmeticExpression ParseAdditiveExpression()
    {
        ArithmeticExpression left = ParseMultiplicativeExpression();
        
        while (currentToken.Type == Tipo.SUM || currentToken.Type == Tipo.SUB)
        {
            Tipo operatorType = currentToken.Type;
            Advance();
            MultiplicativeExpression right = ParseMultiplicativeExpression();
            left = new AdditiveExpression(left, right, operatorType, left.Line, left.Column);
        }
        
        return left;
    }
    
    private ArithmeticExpression ParseMultiplicativeExpression()
    {
        ArithmeticExpression left = ParsePowerExpression();
        
        while (currentToken.Type == Tipo.MULT || 
               currentToken.Type == Tipo.DIV || 
               currentToken.Type == Tipo.MOD)
        {
            Tipo operatorType = currentToken.Type;
            Advance();
            PowerExpression right = ParsePowerExpression();
            left = new MultiplicativeExpression(left, right, operatorType, left.Line, left.Column);
        }
        
        return left;
    }
    
    private ArithmeticExpression ParsePowerExpression()
    {
        ArithmeticExpression base_ = ParseUnaryExpression();
        
        if (currentToken.Type == Tipo.POW)
        {
            Advance();
            UnaryExpressionNode exponent = ParseUnaryExpression();
            return new PowerExpression(base_, exponent, base_.Line, base_.Column);
        }
        
        return base_;
    }
    
    private UnaryExpressionNode ParseUnaryExpression()
    {
        bool negative = false;
        int line = currentToken.Line;
        int column = currentToken.Column;
        
        while (currentToken.Type == Tipo.SUB)
        {
            negative = !negative; // Alternar el signo
            Advance();
        }
        
        PrimaryExpression expr = ParsePrimaryExpression();
        return new UnaryExpressionNode(negative, expr, line, column);
    }
    
    
    







    // Parseo de expresiones booleanas
    private BooleanExpression ParseBooleanExpression()
    {
        return ParseOrExpression();
    }
    
    private OrExpression ParseOrExpression()
    {
        AndExpression left = ParseAndExpression();
        
        while (currentToken.Type == Tipo.OR)
        {
            Advance();
            AndExpression right = ParseAndExpression();
            left = new OrExpression(left, right, left.Line, left.Column);
        }
        
        return left;
    }
    
    private AndExpression ParseAndExpression()
    {
        ComparisonExpression left = ParseComparisonExpression();
        
        while (currentToken.Type == Tipo.AND)
        {
            Advance();
            ComparisonExpression right = ParseComparisonExpression();
            left = new AndExpression(left, right, left.Line, left.Column);
        }
        
        return left;
    }
    
    private ComparisonExpression ParseComparisonExpression()
    {
        ArithmeticExpression left = ParseArithmeticExpression();
        
        if (IsBooleanOperator(currentToken.Type))
        {
            Tipo operatorType = currentToken.Type;
            Advance();
            ArithmeticExpression right = ParseArithmeticExpression();
            return new ComparisonExpression(left, operatorType, right, left.Line, left.Column);
        }
        else if (currentToken.Type == Tipo.L_PAREN)
        {
            Consume(Tipo.L_PAREN);
            BooleanExpression expr = ParseBooleanExpression();
            Consume(Tipo.R_PAREN);
            return (ComparisonExpression)expr;
        }
        else
        {
            throw new Exception($"Se esperaba un operador de comparación en línea {currentToken.Line}, columna {currentToken.Column}");
        }
    }
}