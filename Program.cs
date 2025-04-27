using System.Text;

string input = File.ReadAllText("./Input.pw");
Lexer lexer = new Lexer(input);


List<Token> processed = lexer.RunLexer();
// foreach (var item in processed)
// {
//     System.Console.WriteLine($"{item.Lexeme} : {item.Type}, Line {item.Line}, Col {item.Column}");
// }

if(lexer.Error != "")System.Console.WriteLine(lexer.Error);
else
{
    Parser parser = new Parser(processed);
    try
    {
        ASTNode ast = parser.Parse();
        ASTVisualizer.Visualize(ast);
    }
    catch(Exception e)
    {
        System.Console.WriteLine(e.Message);
    }
}
