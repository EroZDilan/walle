using System;
using System.Collections.Generic;

public class SemanticAnalyzer
{
    private readonly ProgramNode ast;
    private Dictionary<string, ExpressionNode> variables;
    private Dictionary<string, LabelNode> labels;
    private int canvasSize;
    
    public string Error { get; private set; }
    
    public SemanticAnalyzer(ProgramNode ast, int canvasSize)
    {
        this.ast = ast;
        this.canvasSize = canvasSize;
        this.variables = new Dictionary<string, ExpressionNode>();
        this.labels = new Dictionary<string, LabelNode>();
        this.Error = "";
    }
    
    public bool Analyze()
    {
        try
        {
            // Primer paso: recolectar todas las etiquetas
            CollectLabels(ast.Statements);

            //Segundo paso: verificar que el programa empiece con una instrucción Spawn
            if(ast.Statements[0] is Instruction instruction && instruction.Name == "Spawn")
            {
                AnalyzeSpawnInstruction(instruction);
            }
            
            else
            {
                Error = "Error semántico: El programa debe comenzar con una instrucción Spawn.";
                return false;
            }
            
            // Tercer paso: verificar la semántica de cada statement
            for (int i = 1; i < ast.Statements.Count; i++)
            {
                AnalyzeStatement(ast.Statements[i]);
            }
            
            return string.IsNullOrEmpty(Error);
        }
        catch (Exception ex)
        {
            Error = $"Error semántico: {ex.Message}";
            return false;
        }
    }
    
    private void CollectLabels(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            if (statement is LabelNode labelNode)
            {
                if (labels.ContainsKey(labelNode.Name))
                {
                    Error += $"Error semántico: Etiqueta '{labelNode.Name}' ya definida en línea {labelNode.Line}, columna {labelNode.Column}.\n";
                }
                else
                {
                    labels[labelNode.Name] = labelNode;
                    
                    // Recolectar etiquetas en los statements de la etiqueta
                    CollectLabels(labelNode.Programa.Statements);
                }
            }
        }
    }
    
    private void AnalyzeStatement(Statement statement)
    {
        if (statement is AssigmentNode assignmentNode)
        {
            AnalyzeAssignment(assignmentNode);
        }
        else if (statement is Instruction instruction)
        {
            AnalyzeInstruction(instruction);
        }
        else if (statement is LabelNode labelNode)
        {
            // Las etiquetas ya fueron recolectadas, ahora analizamos sus statements
            foreach (var innerStatement in labelNode.Programa.Statements)
            {
                AnalyzeStatement(innerStatement);
            }
        }
        else if (statement is FunctionCallStatement functionCallStatement)
        {
            AnalyzeFunctionCall(functionCallStatement.FunctionCall);
        }
    }
    
    private void AnalyzeAssignment(AssigmentNode assignment)
    {
        // Verificar que la expresión sea válida
        ExpressionNode expressionValue = AnalyzeExpression(assignment.Value);
        
        // Guardar el tipo de la variable
        variables[assignment.VariableName] = expressionValue;
    }
    
    private void AnalyzeInstruction(Instruction instruction)
    {
        string instructionName = instruction.Name;
        
        switch (instructionName)
        {       
            case "Color":
                AnalyzeColorInstruction(instruction);
                break;
                
            case "Size":
                AnalyzeSizeInstruction(instruction);
                break;
                
            case "DrawLine":
                AnalyzeDrawLineInstruction(instruction);
                break;
                
            case "DrawCircle":
                AnalyzeDrawCircleInstruction(instruction);
                break;
                
            case "DrawRectangle":
                AnalyzeDrawRectangleInstruction(instruction);
                break;
                
            case "Fill":
                AnalyzeFillInstruction(instruction);
                break;
                
            case "GoTo":
                AnalyzeGoToInstruction(instruction);
                break;
                
            default:
                Error += $"Error semántico: Instrucción invalida '{instructionName}' en línea {instruction.Line}, columna {instruction.Column}.\n";
                break;
        }
    }
    
    private void AnalyzeSpawnInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 2 argumentos
        if (instruction.Arguments.Count != 2)
        {
            Error += $"Error semántico: Instrucción Spawn debe tener exactamente 2 argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que los argumentos sean expresiones numéricas
        ExpressionNode x = AnalyzeExpression(instruction.Arguments[0]);
        ExpressionNode y = AnalyzeExpression(instruction.Arguments[1]);
        
        if (!(x is ArithmeticExpression) || !(y is ArithmeticExpression))
        {
            Error += $"Error semántico: Argumentos de Spawn deben ser expresiones numéricas en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que las coordenadas estén dentro del canvas
        // Idealmente esto se haría en tiempo de ejecución, pero para algunos casos podemos
        // verificarlo en tiempo de compilación si son constantes.
        
        // Comentado por Karla bajo su propia responsabilidad
        // if (x is NumberNode xNode && y is NumberNode yNode)
        // {
        //     if (xNode.Value < 0 || xNode.Value >= canvasSize || 
        //         yNode.Value < 0 || yNode.Value >= canvasSize)
        //     {
        //         Error += $"Error semántico: Coordenadas de Spawn ({xNode.Value}, {yNode.Value}) fuera de los límites del canvas en línea {instruction.Line}, columna {instruction.Column}.\n";
        //     }
        // }
    }
    
    private void AnalyzeColorInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 1 argumento
        if (instruction.Arguments.Count != 1)
        {
            Error += $"Error semántico: Instrucción Color debe tener exactamente 1 argumento en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que el argumento sea una expresión de cadena
        ExpressionNode colorArg = AnalyzeExpression(instruction.Arguments[0]);
        
        if (!(colorArg is StringExpression))
        {
            Error += $"Error semántico: Argumento de Color debe ser una cadena en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que sea un color válido
        StringExpression colorExpr = (StringExpression)colorArg;
        string color = colorExpr.Value.Trim('"'); // Quitar comillas
        
        if (!IsValidColor(color))
        {
            Error += $"Error semántico: Color '{color}' no válido en línea {instruction.Line}, columna {instruction.Column}. Colores válidos: Red, Blue, Green, Yellow, Orange, Purple, Black, White, Transparent.\n";
        }
    }
    
    private bool IsValidColor(string color)
    {
        string[] validColors = 
        { 
            "Red", "Blue", "Green", "Yellow", 
            "Orange", "Purple", "Black", "White", "Transparent" 
        };
        
        return Array.Exists(validColors, c => c == color);
    }
    
    private void AnalyzeSizeInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 1 argumento
        if (instruction.Arguments.Count != 1)
        {
            Error += $"Error semántico: Instrucción Size debe tener exactamente 1 argumento en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que el argumento sea una expresión numérica
        ExpressionNode sizeArg = AnalyzeExpression(instruction.Arguments[0]);
        
        if (!(sizeArg is ArithmeticExpression))
        {
            Error += $"Error semántico: Argumento de Size debe ser una expresión numérica en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que el tamaño sea positivo si es una constante
        if (sizeArg is NumberNode sizeNode && sizeNode.Value <= 0)
        {
            Error += $"Error semántico: Tamaño del pincel debe ser positivo en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private void AnalyzeDrawLineInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 3 argumentos
        if (instruction.Arguments.Count != 3)
        {
            Error += $"Error semántico: Instrucción DrawLine debe tener exactamente 3 argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que los argumentos sean expresiones numéricas
        ExpressionNode dirX = AnalyzeExpression(instruction.Arguments[0]);
        ExpressionNode dirY = AnalyzeExpression(instruction.Arguments[1]);
        ExpressionNode distance = AnalyzeExpression(instruction.Arguments[2]);
        
        if (!(dirX is ArithmeticExpression) || !(dirY is ArithmeticExpression) || !(distance is ArithmeticExpression))
        {
            Error += $"Error semántico: Argumentos de DrawLine deben ser expresiones numéricas en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que las direcciones sean -1, 0 o 1 si son constantes
        if (dirX is NumberNode dirXNode && dirY is NumberNode dirYNode)
        {
            if (dirXNode.Value < -1 || dirXNode.Value > 1 || 
                dirYNode.Value < -1 || dirYNode.Value > 1)
            {
                Error += $"Error semántico: Direcciones para DrawLine deben ser -1, 0 o 1 en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
            
            // Verificar que no sea (0, 0)
            if (dirXNode.Value == 0 && dirYNode.Value == 0)
            {
                Error += $"Error semántico: Dirección para DrawLine no puede ser (0, 0) en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
        }
        
        // Verificar que la distancia sea positiva si es una constante
        if (distance is NumberNode distanceNode && distanceNode.Value <= 0)
        {
            Error += $"Error semántico: Distancia para DrawLine debe ser positiva en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private void AnalyzeDrawCircleInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 3 argumentos
        if (instruction.Arguments.Count != 3)
        {
            Error += $"Error semántico: Instrucción DrawCircle debe tener exactamente 3 argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que los argumentos sean expresiones numéricas
        ExpressionNode dirX = AnalyzeExpression(instruction.Arguments[0]);
        ExpressionNode dirY = AnalyzeExpression(instruction.Arguments[1]);
        ExpressionNode radius = AnalyzeExpression(instruction.Arguments[2]);
        
        if (!(dirX is ArithmeticExpression) || !(dirY is ArithmeticExpression) || !(radius is ArithmeticExpression))
        {
            Error += $"Error semántico: Argumentos de DrawCircle deben ser expresiones numéricas en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que las direcciones sean -1, 0 o 1 si son constantes
        if (dirX is NumberNode dirXNode && dirY is NumberNode dirYNode)
        {
            if (dirXNode.Value < -1 || dirXNode.Value > 1 || 
                dirYNode.Value < -1 || dirYNode.Value > 1)
            {
                Error += $"Error semántico: Direcciones para DrawCircle deben ser -1, 0 o 1 en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
            
            // Verificar que no sea (0, 0)
            if (dirXNode.Value == 0 && dirYNode.Value == 0)
            {
                Error += $"Error semántico: Dirección para DrawCircle no puede ser (0, 0) en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
        }
        
        // Verificar que el radio sea positivo si es una constante
        if (radius is NumberNode radiusNode && radiusNode.Value <= 0)
        {
            Error += $"Error semántico: Radio para DrawCircle debe ser positivo en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private void AnalyzeDrawRectangleInstruction(Instruction instruction)
    {
        // Verificar que tenga exactamente 5 argumentos
        if (instruction.Arguments.Count != 5)
        {
            Error += $"Error semántico: Instrucción DrawRectangle debe tener exactamente 5 argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que los argumentos sean expresiones numéricas
        ExpressionNode dirX = AnalyzeExpression(instruction.Arguments[0]);
        ExpressionNode dirY = AnalyzeExpression(instruction.Arguments[1]);
        ExpressionNode distance = AnalyzeExpression(instruction.Arguments[2]);
        ExpressionNode width = AnalyzeExpression(instruction.Arguments[3]);
        ExpressionNode height = AnalyzeExpression(instruction.Arguments[4]);
        
        if (!(dirX is ArithmeticExpression) || !(dirY is ArithmeticExpression) || 
            !(distance is ArithmeticExpression) || !(width is ArithmeticExpression) || 
            !(height is ArithmeticExpression))
        {
            Error += $"Error semántico: Argumentos de DrawRectangle deben ser expresiones numéricas en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // Verificar que las direcciones sean -1, 0 o 1 si son constantes
        if (dirX is NumberNode dirXNode && dirY is NumberNode dirYNode)
        {
            if (dirXNode.Value < -1 || dirXNode.Value > 1 || 
                dirYNode.Value < -1 || dirYNode.Value > 1)
            {
                Error += $"Error semántico: Direcciones para DrawRectangle deben ser -1, 0 o 1 en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
            
            // Verificar que no sea (0, 0)
            if (dirXNode.Value == 0 && dirYNode.Value == 0)
            {
                Error += $"Error semántico: Dirección para DrawRectangle no puede ser (0, 0) en línea {instruction.Line}, columna {instruction.Column}.\n";
            }
        }
        
        // Verificar que distancia, ancho y alto sean positivos si son constantes
        if (distance is NumberNode distanceNode && distanceNode.Value <= 0)
        {
            Error += $"Error semántico: Distancia para DrawRectangle debe ser positiva en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
        
        if (width is NumberNode widthNode && widthNode.Value <= 0)
        {
            Error += $"Error semántico: Ancho para DrawRectangle debe ser positivo en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
        
        if (height is NumberNode heightNode && heightNode.Value <= 0)
        {
            Error += $"Error semántico: Alto para DrawRectangle debe ser positivo en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private void AnalyzeFillInstruction(Instruction instruction)
    {
        // Verificar que no tenga argumentos
        if (instruction.Arguments.Count != 0)
        {
            Error += $"Error semántico: Instrucción Fill no debe tener argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private void AnalyzeGoToInstruction(Instruction instruction)
    {
        // Debe tener exactamente 2 argumentos: la etiqueta y la condición
        if (instruction.Arguments.Count != 2)
        {
            Error += $"Error semántico: Instrucción GoTo debe tener exactamente 2 argumentos en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // El primer argumento debe ser una cadena con el nombre de la etiqueta
        if (!(instruction.Arguments[0] is StringExpression labelExpr))
        {
            Error += $"Error semántico: El primer argumento de GoTo debe ser el nombre de la etiqueta en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        string labelName = labelExpr.Value.Trim('"'); // Quitar comillas
        
        // Verificar que la etiqueta exista
        if (!labels.ContainsKey(labelName))
        {
            Error += $"Error semántico: Etiqueta '{labelName}' no definida en línea {instruction.Line}, columna {instruction.Column}.\n";
            return;
        }
        
        // El segundo argumento debe ser una expresión booleana
        ExpressionNode condition = AnalyzeExpression(instruction.Arguments[1]);
        
        if (!(condition is BooleanExpression))
        {
            Error += $"Error semántico: El segundo argumento de GoTo debe ser una expresión booleana en línea {instruction.Line}, columna {instruction.Column}.\n";
        }
    }
    
    private ExpressionNode AnalyzeExpression(ExpressionNode expression)
    {
        if (expression is StringExpression)
        {
            return expression; // Las expresiones de cadena son válidas tal cual
        }
        else if (expression is ArithmeticExpression)
        {
            return AnalyzeArithmeticExpression((ArithmeticExpression)expression);
        }
        else if (expression is BooleanExpression)
        {
            return AnalyzeBooleanExpression((BooleanExpression)expression);
        }
        else if (expression is BoolLiteralNode)
        {
            return expression; // Los literales booleanos son válidos tal cual
        }
        
        Error += $"Error semántico: Tipo de expresión no reconocido en línea {expression.Line}, columna {expression.Column}.\n";
        return expression;
    }
    
    private ArithmeticExpression AnalyzeArithmeticExpression(ArithmeticExpression expression)
    {
        if (expression is NumberNode)
        {
            return expression; // Los números son válidos tal cual
        }
        else if (expression is VariableNode variableNode)
        {
            // Verificar que la variable esté definida
            if (!variables.ContainsKey(variableNode.Name))
            {
                Error += $"Error semántico: Variable '{variableNode.Name}' no definida en línea {variableNode.Line}, columna {variableNode.Column}.\n";
                return expression;
            }
            
            // Verificar que la variable sea de tipo numérico
            ExpressionNode varValue = variables[variableNode.Name];
            if (!(varValue is ArithmeticExpression))
            {
                Error += $"Error semántico: Variable '{variableNode.Name}' no es de tipo numérico en línea {variableNode.Line}, columna {variableNode.Column}.\n";
            }
            
            return expression;
        }
        else if (expression is FunctionCall functionCall)
        {
            return AnalyzeFunctionCall(functionCall);
        }
        else if (expression is AdditiveExpression additiveExpr)
        {
            AnalyzeArithmeticExpression(additiveExpr.Left);
            AnalyzeArithmeticExpression(additiveExpr.Right!);
            return expression;
        }
        else if (expression is MultiplicativeExpression multExpr)
        {
            AnalyzeArithmeticExpression(multExpr.Left);
            AnalyzeArithmeticExpression(multExpr.Right!);
            return expression;
        }
        else if (expression is PowerExpression powerExpr)
        {
            AnalyzeArithmeticExpression(powerExpr.Base);
            if (powerExpr.Exponent != null)
            {
                AnalyzeArithmeticExpression(powerExpr.Exponent);
            }
            return expression;
        }
        else if (expression is UnaryExpressionNode unaryExpr)
        {
            AnalyzeArithmeticExpression(unaryExpr.Module);
            return expression;
        }
        
        Error += $"Error semántico: Tipo de expresión aritmética no reconocido en línea {expression.Line}, columna {expression.Column}.\n";
        return expression;
    }
    
    private BooleanExpression AnalyzeBooleanExpression(BooleanExpression expression)
    {
        if (expression is OrExpression orExpr)
        {
            AnalyzeBooleanExpression(orExpr.Left);
            AnalyzeBooleanExpression(orExpr.Right!);
            return expression;
        }
        else if (expression is AndExpression andExpr)
        {
            AnalyzeBooleanExpression(andExpr.Left);
            AnalyzeBooleanExpression(andExpr.Right!);
            return expression;
        }
        else if (expression is ComparisonExpression compExpr)
        {
            AnalyzeExpression(compExpr.Left);
            AnalyzeExpression(compExpr.Right!);
            return expression;
        }
        
        Error += $"Error semántico: Tipo de expresión booleana no reconocido en línea {expression.Line}, columna {expression.Column}.\n";
        return expression;
    }
    
    private ArithmeticExpression AnalyzeFunctionCall(FunctionCall functionCall)
    {
        string functionName = GetFunctionName(functionCall);
        
        switch (functionName)
        {
            case "GetActualX":
                VerifyArgumentCount(functionCall, 0);
                break;
                
            case "GetActualY":
                VerifyArgumentCount(functionCall, 0);
                break;
                
            case "GetCanvasSize":
                VerifyArgumentCount(functionCall, 0);
                break;
                
            case "GetColorCount":
                AnalyzeGetColorCount(functionCall);
                break;
                
            case "IsBrushColor":
                AnalyzeIsBrushColor(functionCall);
                break;
                
            case "IsBrushSize":
                AnalyzeIsBrushSize(functionCall);
                break;
                
            case "IsCanvasColor":
                AnalyzeIsCanvasColor(functionCall);
                break;
                
            default:
                Error += $"Error semántico: Función desconocida '{functionName}' en línea {functionCall.Line}, columna {functionCall.Column}.\n";
                break;
        }
        
        return functionCall;
    }
    
    private string GetFunctionName(FunctionCall functionCall)
    {
        // En una implementación real, necesitarías acceder al lexema del token
        return functionCall.Name.ToString();
    }
    
    private void VerifyArgumentCount(FunctionCall functionCall, int expectedCount)
    {
        if (functionCall.Arguments.Count != expectedCount)
        {
            Error += $"Error semántico: Función {GetFunctionName(functionCall)} debe tener exactamente {expectedCount} argumentos en línea {functionCall.Line}, columna {functionCall.Column}.\n";
        }
    }
    
    private void AnalyzeGetColorCount(FunctionCall functionCall)
    {
        // Verificar que tenga exactamente 5 argumentos
        if (functionCall.Arguments.Count != 5)
        {
            Error += $"Error semántico: Función GetColorCount debe tener exactamente 5 argumentos en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el primer argumento sea una cadena (color)
        if (!(functionCall.Arguments[0] is StringExpression))
        {
            Error += $"Error semántico: El primer argumento de GetColorCount debe ser una cadena (color) en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el color sea válido
        StringExpression colorExpr = (StringExpression)functionCall.Arguments[0];
        string color = colorExpr.Value.Trim('"'); // Quitar comillas
        
        if (!IsValidColor(color))
        {
            Error += $"Error semántico: Color '{color}' no válido en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que los otros 4 argumentos sean expresiones numéricas
        for (int i = 1; i <= 4; i++)
        {
            ExpressionNode arg = AnalyzeExpression(functionCall.Arguments[i]);
            
            if (!(arg is ArithmeticExpression))
            {
                Error += $"Error semántico: El argumento {i+1} de GetColorCount debe ser una expresión numérica en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            }
        }
    }
    
    private void AnalyzeIsBrushColor(FunctionCall functionCall)
    {
        // Verificar que tenga exactamente 1 argumento
        if (functionCall.Arguments.Count != 1)
        {
            Error += $"Error semántico: Función IsBrushColor debe tener exactamente 1 argumento en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el argumento sea una cadena (color)
        if (!(functionCall.Arguments[0] is StringExpression))
        {
            Error += $"Error semántico: El argumento de IsBrushColor debe ser una cadena (color) en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el color sea válido
        StringExpression colorExpr = (StringExpression)functionCall.Arguments[0];
        string color = colorExpr.Value.Trim('"'); // Quitar comillas
        
        if (!IsValidColor(color))
        {
            Error += $"Error semántico: Color '{color}' no válido en línea {functionCall.Line}, columna {functionCall.Column}.\n";
        }
    }
    
    private void AnalyzeIsBrushSize(FunctionCall functionCall)
    {
        // Verificar que tenga exactamente 1 argumento
        if (functionCall.Arguments.Count != 1)
        {
            Error += $"Error semántico: Función IsBrushSize debe tener exactamente 1 argumento en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el argumento sea una expresión numérica
        ExpressionNode sizeArg = AnalyzeExpression(functionCall.Arguments[0]);
        
        if (!(sizeArg is ArithmeticExpression))
        {
            Error += $"Error semántico: El argumento de IsBrushSize debe ser una expresión numérica en línea {functionCall.Line}, columna {functionCall.Column}.\n";
        }
    }
    
    private void AnalyzeIsCanvasColor(FunctionCall functionCall)
    {
        // Verificar que tenga exactamente 3 argumentos
        if (functionCall.Arguments.Count != 3)
        {
            Error += $"Error semántico: Función IsCanvasColor debe tener exactamente 3 argumentos en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el primer argumento sea una cadena (color)
        if (!(functionCall.Arguments[0] is StringExpression))
        {
            Error += $"Error semántico: El primer argumento de IsCanvasColor debe ser una cadena (color) en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        
        // Verificar que el color sea válido
        StringExpression colorExpr = (StringExpression)functionCall.Arguments[0];
        string color = colorExpr.Value.Trim('"'); // Quitar comillas
        
        if (!IsValidColor(color))
        {
            Error += $"Error semántico: Color '{color}' no válido en línea {functionCall.Line}, columna {functionCall.Column}.\n";
            return;
        }
        // Verificar que los otros 2 argumentos sean expresiones numéricas
        ExpressionNode verticalArg = AnalyzeExpression(functionCall.Arguments[1]);
        ExpressionNode horizontalArg = AnalyzeExpression(functionCall.Arguments[2]);
        
        if (!(verticalArg is ArithmeticExpression) || !(horizontalArg is ArithmeticExpression))
        {
            Error += $"Error semántico: Los argumentos 2 y 3 de IsCanvasColor deben ser expresiones numéricas en línea {functionCall.Line}, columna {functionCall.Column}.\n";
        }
    }
}