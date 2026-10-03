using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

class ItemSlice : CSharpSyntaxRewriter {
    readonly HashSet<string> fields = new() {"createTile", "placeStyle", "maxStack", "mech"};
    readonly HashSet<string> helpers = new() {"DefaultToPlaceableTile", "DefaultToTorch", "DefaultToMusicBox", "DefaultToMonolith", "DefaultToBanner", "SetDefaults1", "SetDefaults2", "SetDefaults3", "SetDefaults4", "SetDefaults5"};
    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node) {
        if (node.Expression is AssignmentExpressionSyntax a && fields.Contains(a.Left.ToString())) return node;
        if (node.Expression is InvocationExpressionSyntax c && helpers.Contains(c.Expression.ToString())) return node;
        return null;
    }
    public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) => null;
    public override SyntaxNode? VisitIfStatement(IfStatementSyntax n) {
        var yes=(StatementSyntax?)Visit(n.Statement) ?? Block();
        var no=n.Else==null?null:(StatementSyntax?)Visit(n.Else.Statement);
        if (!n.Condition.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(x=>x.Identifier.Text=="type")) {
            bool affects=yes.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Any(a=>fields.Contains(a.Left.ToString())) || yes.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any(c=>helpers.Contains(c.Expression.ToString()));
            if (affects) throw new Exception("Unresolved placement condition: "+n.Condition);
            return null;
        }
        return n.WithStatement(yes).WithElse(no==null?null:ElseClause(no));
    }
    public override SyntaxNode? VisitSwitchStatement(SwitchStatementSyntax n) {
        if (n.Expression.ToString()!="type") return null;
        return base.VisitSwitchStatement(n);
    }
}
class Program {
 static void Main(string[] args) {
   string root=args[0],outdir=args[1]; Directory.CreateDirectory(outdir);
   var src=CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root,"Terraria/Item.cs")));
   var methods=src.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m=>Enumerable.Range(1,5).Select(i=>"SetDefaults"+i).Contains(m.Identifier.Text)).ToArray();
   if(methods.Length!=5) throw new Exception("Expected five defaults methods");
   var rw=new ItemSlice();
   string text="using System; using System.Collections.Generic; public static class BoolExtensions { public static int ToInt(this bool b) => b?1:0; } public class ExtractedItem { public int createTile=-1,placeStyle=0,maxStack=9999; public bool mech=false; "+
    "void DefaultToPlaceableTile(int tileIDToPlace,int tileStyleToPlace=0) {createTile=tileIDToPlace;placeStyle=tileStyleToPlace;} void DefaultToTorch(int tileStyleToPlace,bool allowWaterPlacement=false){createTile=4;placeStyle=tileStyleToPlace;} void DefaultToMusicBox(int style){createTile=139;placeStyle=style;} void DefaultToMonolith(int tileIDToPlace,int tileStyleToPlace=0){DefaultToPlaceableTile(tileIDToPlace,tileStyleToPlace);} void DefaultToBanner(int tileStyleToPlace=0){DefaultToPlaceableTile(91,tileStyleToPlace);} "+
    "public void Run(int type){if(type<=1000)SetDefaults1(type);else if(type<=2001)SetDefaults2(type);else if(type<=3000)SetDefaults3(type);else if(type<=3989)SetDefaults4(type);else SetDefaults5(type);}"+
    string.Join("\n",methods.Select(m=>rw.Visit(m)!.NormalizeWhitespace().ToFullString()))+"}";
   File.WriteAllText(Path.Combine(outdir,"ItemDefaults.slice.cs.txt"),text);
   var refs=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p));
   var compilation=CSharpCompilation.Create("PlacementSlice",new[]{CSharpSyntaxTree.ParseText(text)},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
   using var stream=new MemoryStream();var result=compilation.Emit(stream);
   var errors=result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
   if(errors.Length>0) throw new Exception(string.Join("\n",errors.Select(e=>e.ToString())));
   var assembly=Assembly.Load(stream.ToArray());var type=assembly.GetType("ExtractedItem")!;
   var countMatch=System.Text.RegularExpressions.Regex.Match(File.ReadAllText(Path.Combine(root,"Terraria.ID/ItemID.cs")),@"public (?:static readonly|const) (?:short|int) Count = (\d+)");
   if(!countMatch.Success)throw new Exception("ItemID.Count missing");int count=int.Parse(countMatch.Groups[1].Value);
   var rows=new List<object>();
   for(int id=1;id<count;id++) { var obj=Activator.CreateInstance(type)!;type.GetMethod("Run")!.Invoke(obj,new object[]{id}); rows.Add(new { id,createTile=(int)type.GetField("createTile")!.GetValue(obj)!,placeStyle=(int)type.GetField("placeStyle")!.GetValue(obj)!,maxStack=(int)type.GetField("maxStack")!.GetValue(obj)!,mech=(bool)type.GetField("mech")!.GetValue(obj)! }); }
   File.WriteAllText(Path.Combine(outdir,"placements.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
   Console.WriteLine("Executed source control-flow slice for "+rows.Count+" item IDs.");
 }
}
