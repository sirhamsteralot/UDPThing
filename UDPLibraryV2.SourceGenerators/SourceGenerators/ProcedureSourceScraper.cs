using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
            Debug.WriteLine("Execute code generator");

            AnnotatedMethodReceiver syntaxReceiver = (AnnotatedMethodReceiver)context.SyntaxReceiver;

            INamedTypeSymbol procedureAttribute = context.Compilation.GetTypeByMetadataName("UDPLibraryV2.RPC.Attributes.Procedure");

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($@"
namespace UDPLibraryV2.RPC {{
    public class ProcedureResolver {{
        public void RegisterProcedures(RPCService rpc) {{
        }}
    }}
}}");
            foreach (MethodDeclarationSyntax method in syntaxReceiver.AnnotatedMethods)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(method.SyntaxTree);
                ISymbol methodSymbol = semanticModel.GetDeclaredSymbol(method);

                ImmutableArray<AttributeData> attributes = methodSymbol.GetAttributes();

                if (attributes.Length < 1)
                    continue;

                AttributeData attribute = attributes.Single(x => procedureAttribute.Equals(x.AttributeClass, SymbolEqualityComparer.Default));

                foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
                {
                    // Is this the ExtensionClassName argument?
                    if (namedArgument.Key == "RequestType")
                    {
                        Debug.WriteLine(((Type)namedArgument.Value.Value).FullName);
                    }
                }
            }

            context.AddSource($"procedureResolver.g.cs", stringBuilder.ToString());

            //throw new NotImplementedException();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            Debug.WriteLine("Initalize code generator");

            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new AnnotatedMethodReceiver());
        }
    }
}
