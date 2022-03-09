using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.RPC.SourceGenerators
{
    internal class AnnotatedMethodReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> AnnotatedMethods { get; private set; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is MethodDeclarationSyntax cds)
            {
                AnnotatedMethods.Add(cds);
            }
        }
    }
}
