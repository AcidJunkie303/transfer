using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AcidJunkie.Analyzers.Tests;

internal sealed class Walker : CSharpSyntaxWalker
{
    private int _level;

    public List<(int Level, string Kind, string TypeName, SyntaxNode Node)> Nodes = new(250);

    public override void Visit(SyntaxNode? node)
    {
        _level++;

        if (node is not null)
        {
            Nodes.Add((_level, node.Kind().ToString(), node.GetType().Name, node));
        }

        base.Visit(node);
        _level--;
    }
}
