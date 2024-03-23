﻿using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnionGen;

[Generator]
public sealed class UnionSourceGen : IIncrementalGenerator
{
    internal const string AutoGeneratedComment = $"// <auto-generated by {nameof(UnionSourceGen)} />";
    private const string AttributeName = "UnionAttribute";
    private const int MinTypeParameters = 2;
    private const int MaxTypeParameters = 6;
    private const int NumberOfAttributes = MaxTypeParameters - MinTypeParameters + 1;

    private static readonly string attributeSourceCode;
    private static readonly string[] attributeClasses;
    private static readonly string genNamespace;

    private static readonly Regex nameFromFullNameRegex = new Regex(@"^.*\.(\D[\d\D]*)$", RegexOptions.Compiled);

    static UnionSourceGen()
    {
        genNamespace = typeof(UnionSourceGen).Namespace
                       ?? throw new InvalidOperationException("Namespace not found");
        (attributeSourceCode, attributeClasses) = CreateAttributeCode(genNamespace);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var unionsToGenerate = new List<IncrementalValuesProvider<UnionToGenerate?>>(NumberOfAttributes);

        foreach (var attributeClass in attributeClasses)
        {
            var unions = context.SyntaxProvider
                                .ForAttributeWithMetadataName(attributeClass,
                                                              static (_, _) => true,
                                                              static (ctx, _) =>
                                                                  GetUnionToGenerate(ctx.SemanticModel, ctx.TargetNode))
                                .Where(static m => m is not null);
            unionsToGenerate.Add(unions);
        }

        foreach (var unions in unionsToGenerate)
        {
            context.RegisterSourceOutput(unions, static (spc, source) => Execute(source, spc));
        }

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource($"{AttributeName}.g.cs",
                                                                      SourceText.From(attributeSourceCode,
                                                                       Encoding.UTF8)));
    }

    private static (string, string[]) CreateAttributeCode(string ns)
    {
        var classes = new IndentedStringBuilder(1);
        var attributeTypeNames = new List<string>(NumberOfAttributes);

        var typeParams = new StringBuilder("T1");
        for (var i = MinTypeParameters; i <= MaxTypeParameters; i++)
        {
            typeParams.Append($", T{i}");

            classes.AppendLine("[AttributeUsage(AttributeTargets.Struct)]");
            classes.AppendLine($"public sealed class {AttributeName}<{typeParams}> : Attribute {{}}");
            attributeTypeNames.Add($"{ns}.{AttributeName}`{i}");
        }

        var sourceCode = $$"""
                           {{AutoGeneratedComment}}
                           #nullable enable
                           namespace UnionGen {

                           {{classes}}
                           }
                           """;

        return (sourceCode, attributeTypeNames.ToArray());
    }

    private static void Execute(UnionToGenerate? sourceData, SourceProductionContext ctx)
    {
        if (sourceData is null)
        {
            return;
        }

        var helper = new UnionGenHelper(sourceData);
        var result = helper.GeneratePartialStruct();
        ctx.AddSource($"{genNamespace}.{sourceData.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static UnionToGenerate? GetUnionToGenerate(SemanticModel semanticModel, SyntaxNode syntaxNode)
    {
        if (syntaxNode is not StructDeclarationSyntax structSyntax)
        {
            return null;
        }

        GenericNameSyntax? genericName = null;
        foreach (var attributeList in structSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeList.Attributes)
            {
                if (attributeSyntax.Name is GenericNameSyntax { Arity: >= 2 } gName
                    && gName.Identifier.ValueText.StartsWith("Union"))
                {
                    genericName = gName;

                    break;
                }
            }

            if (genericName != null)
            {
                break;
            }
        }

        if (genericName is null)
        {
            return null;
        }

        var typeNames = GetTypeParameters(genericName, semanticModel);
        if (typeNames is null)
        {
            return null;
        }

        var annotatedType = structSyntax.Identifier.Text;
        var structSymbol = semanticModel.GetDeclaredSymbol(structSyntax);

        if (structSymbol?.ContainingNamespace is null
            || string.IsNullOrWhiteSpace(structSymbol.ContainingNamespace.Name))
        {
            return null;
        }

        var structNamespace = structSymbol.ContainingNamespace.ToDisplayString();

        return new UnionToGenerate(annotatedType, structNamespace, new(typeNames));
    }

    private static List<TypeParameter>? GetTypeParameters(GenericNameSyntax genericName, SemanticModel semanticModel)
    {
        var typeNames = new List<TypeParameter>(genericName.TypeArgumentList.Arguments.Count);
        foreach (var argument in genericName.TypeArgumentList.Arguments)
        {
            var typeSymbol = semanticModel.GetTypeInfo(argument).Type;
            if (typeSymbol is null)
            {
                return null;
            }

            var name = typeSymbol.Name;
            var fullName = typeSymbol.ToString();
            var isReferenceType = typeSymbol.IsReferenceType;
            var isBuiltInType = typeSymbol.SpecialType != SpecialType.None;
            if (isBuiltInType)
            {
                name = fullName;
            }

            if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
            {
                var typeArguments = namedTypeSymbol.TypeArguments;
                name = typeArguments.Length switch
                       {
                           1 => $"{name}Of{typeArguments[0].Name.EnsureTitleCase()}",
                           2 =>
                               $"{name}Of{typeArguments[0].Name.EnsureTitleCase()}And{typeArguments[1].Name.EnsureTitleCase()}",
                           _ => $"Generic{name}"
                       };
            }

            name = SanitizeName(name, fullName);

            typeNames.Add(new TypeParameter(name, fullName, isReferenceType));
        }

        return typeNames;
    }

    private static string SanitizeName(string? rawName, string fullName)
    {
        var name = string.IsNullOrWhiteSpace(rawName)
            ? fullName
            : rawName!;

        name = name.Replace("[]", "Array");
        var matches = nameFromFullNameRegex.Matches(name);
        if (matches.Count > 0)
        {
            name = matches[0].Groups[1].Value;
        }

        return name;
    }
}
