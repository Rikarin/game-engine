using Rin.Core.Shaders.Ast;

namespace Rin.Core.Shaders; 

public partial class BuildAstVisitor {
    public override Node VisitStatement_list(RinParser.Statement_listContext context) {
        var statements = new StatementList();
        foreach (var statement in context.statement()) {
            statements.Add((Statement)Visit(statement));
        }

        return statements;
    }

    public override Node VisitStatement(RinParser.StatementContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitEmbedded_statement(RinParser.Embedded_statementContext context) {
        return Visit(context.GetChild(0));
    }


    // : expression                                                    #expressionStatement
    // | IF OPEN_PARENS expression CLOSE_PARENS block (ELSE block)?    #ifStatement
    // | WHILE OPEN_PARENS expression CLOSE_PARENS block               #whileStatement
    // | REPEAT block WHILE OPEN_PARENS expression CLOSE_PARENS        #repeatStatement
    // | FOR OPEN_PARENS identifier IN expression CLOSE_PARENS block   #forStatement
    // | BREAK NL+                                                     #breakStatement
    // | CONTINUE NL+                                                  #continueStatement
    // | RETURN expression? NL+                                        #returnStatement
    
    public override Node VisitExpressionStatement(RinParser.ExpressionStatementContext context) {
        var expression = Visit(context.expression()) as Expression;
        return new ExpressionStatement(expression);
    }
    
    public override Node VisitIfStatement(RinParser.IfStatementContext context) {
        var block = context.block();
        var condition = Visit(context.expression()) as Expression;
        var body = Visit(block[0]) as Statement;
        var elseBody = block.Length == 2 ? Visit(block[1]) as Statement : null;

        return new IfStatement(condition, body, elseBody);
    }

    public override Node VisitWhileStatement(RinParser.WhileStatementContext context) {
        var condition = Visit(context.expression()) as Expression;
        var body = Visit(context.block()) as Statement;

        return new WhileStatement(condition, body);
    }

    public override Node VisitRepeatStatement(RinParser.RepeatStatementContext context) {
        var condition = Visit(context.expression()) as Expression;
        var body = Visit(context.block()) as Statement;

        return new RepeatStatement(condition, body);
    }

    public override Node VisitForStatement(RinParser.ForStatementContext context) {
        var variable = new DeclarationStatement(new Variable(new ValType(), Visit(context.identifier()) as Identifier));
        var condition = Visit(context.expression()) as Expression;
        var body = Visit(context.block()) as Statement;

        return new ForStatement(variable, condition, body);
    }

    public override Node VisitBreakStatement(RinParser.BreakStatementContext context) {
        return new BreakStatement();
    }

    public override Node VisitContinueStatement(RinParser.ContinueStatementContext context) {
        return new ContinueStatement();
    }

    public override Node VisitReturnStatement(RinParser.ReturnStatementContext context) {
        if (context.expression() is { } expression) {
            return new ReturnStatement(Visit(expression) as Expression);
        }
        
        return new ReturnStatement();
    }
}
