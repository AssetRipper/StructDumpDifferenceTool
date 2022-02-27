using System;
using System.IO;

namespace AssetRipper.StructDumpDifferenceTool.ConsoleApp
{
	public static class Program
	{
		private const string ReleaseDumpsPath = @"F:\TypeTreeDumps\StructsDump\release";
		private const string EditorDumpsPath = @"F:\TypeTreeDumps\StructsDump\editor";
		public static void Main(string[] args)
		{
			try
			{
				string outputPath = "Comparisons";
				if(Directory.Exists(outputPath))
					Directory.Delete(outputPath, true);
				StructDumpParser.DoComparisons(EditorDumpsPath, outputPath);
				Console.WriteLine("Done!");
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
			Console.ReadLine();
		}
	}
}