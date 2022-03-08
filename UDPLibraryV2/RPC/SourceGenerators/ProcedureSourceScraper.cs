using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPLibraryV2.RPC.SourceGenerators
{
    [Generator]
    internal class ProcedureSourceScraper : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            AnnotatedClassReceiver syntaxReceiver = (AnnotatedClassReceiver)context.SyntaxReceiver;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($@"
namespace UDPLibraryV2.Utils {{
    public class ProcedureResolver {{
        public T 
");

            foreach (ClassDeclarationSyntax userClass in syntaxReceiver.AnnotatedClasses)
            {

            }

            throw new NotImplementedException();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new AnnotatedClassReceiver());
        }
    }
}
