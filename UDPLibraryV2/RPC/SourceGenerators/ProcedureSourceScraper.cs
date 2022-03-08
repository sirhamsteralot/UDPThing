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
    [Generator]
    internal class ProcedureSourceScraper : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            AnnotatedMethodReceiver syntaxReceiver = (AnnotatedMethodReceiver)context.SyntaxReceiver;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($@"
namespace UDPLibraryV2.RPC {{
    public class ProcedureResolver {{
        public void RegisterProcedures(RPCService rpc) {{
");

            foreach (MethodDeclarationSyntax method in syntaxReceiver.AnnotatedMethods)
            {
                var attribute = method.AttributeLists.SelectMany(x => x.Attributes).Single(x => x.Name.ToFullString() == nameof(Procedure));

            }

            throw new NotImplementedException();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new AnnotatedMethodReceiver());
        }
    }
}
