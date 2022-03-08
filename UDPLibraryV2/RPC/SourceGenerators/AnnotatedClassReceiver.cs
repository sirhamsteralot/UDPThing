using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPLibraryV2.RPC.Attributes;

namespace UDPLibraryV2.RPC.SourceGenerators
{
    internal class AnnotatedClassReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> AnnotatedClasses { get; private set; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds &&
                cds.AttributeLists.Count > 0)
            {
                var sysntaxAttributes = cds.AttributeLists.SelectMany(e => e.Attributes)
                    .Where(e => e.Name.NormalizeWhitespace().ToFullString() == nameof(Procedure));

                AnnotatedClasses.Add(cds);
            }
        }
    }
}
