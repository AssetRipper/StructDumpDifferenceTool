namespace AssetRipper.StructDumpDifferenceTool
{
	public static class StructDumpParser
	{
		private const string DumpExtension = ".dump";

		public static void DoComparisons(string inputDirectory, string outputDirectory)
		{
			string[] files = GetOrderedFilePaths(inputDirectory);
			if (files.Length < 2)
				throw new Exception("Not enough files");

			Console.WriteLine(files[0]);
			Dictionary<int, string> previousClassDictionary = GetClassDictionary(files[0]);
			foreach((int id, string classContents) in previousClassDictionary)
			{
				WriteContentsToFile(id, files[0], outputDirectory, classContents);
			}

			for(int i = 1; i < files.Length; i++)
			{
				Console.WriteLine(files[i]);
				Dictionary<int, string> currentClassDictionary = GetClassDictionary(files[i]);
				IEnumerable<int> keys = currentClassDictionary.Keys.Concat(previousClassDictionary.Keys).Distinct();
				foreach(int key in keys)
				{
					bool isKeyInPrevious = previousClassDictionary.ContainsKey(key);
					bool isKeyInCurrent = currentClassDictionary.ContainsKey(key);
					if(isKeyInCurrent && isKeyInPrevious)
					{
						string previousClass = previousClassDictionary[key];
						string currentClass = currentClassDictionary[key];
						if(!previousClass.Equals(currentClass, StringComparison.Ordinal))
						{
							WriteContentsToFile(key, files[i], outputDirectory, currentClass);
						}
					}
					else if (isKeyInCurrent && !isKeyInPrevious)
					{
						WriteContentsToFile(key, files[i], outputDirectory, currentClassDictionary[key]);
					}
					else if (!isKeyInCurrent && isKeyInPrevious)
					{
						WriteContentsToFile(key, files[i], outputDirectory, "");
					}
					else
					{
						throw new Exception("Should not be possible");
					}
				}
				previousClassDictionary = currentClassDictionary;
			}
		}

		private static void WriteContentsToFile(int id, string sourceFilePath, string outputDirectoryPath, string contents)
		{
			string fileName = Path.GetFileName(sourceFilePath);
			string subDirectory = Path.Combine(outputDirectoryPath, id.ToString());
			Directory.CreateDirectory(subDirectory);
			string outputPath = Path.Combine(subDirectory, fileName);
			File.WriteAllText(outputPath, contents);
		}

		private static string[] GetSplitText(string filePath)
		{
			string fileText = File.ReadAllText(filePath);
			//note: struct dump files are encoded with crlf line endings, but the other two are included here for robustness
			return fileText.Split(new string[] { "\r\r", "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static Dictionary<int, string> GetClassDictionary(string filePath)
		{
			return GetClassDictionary(GetSplitText(filePath));
		}

		private static Dictionary<int, string> GetClassDictionary(string[] splitClasses)
		{
			Dictionary<int, string> result = new Dictionary<int, string>();
			for(int i = 0; i < splitClasses.Length; i++)
			{
				string section = splitClasses[i].Trim();
				if(!section.StartsWith("// classID{", StringComparison.Ordinal))
				{
					throw new Exception(section);
				}

				const int leftIndex = 10;
				int rightIndex = section.IndexOf('}', StringComparison.Ordinal);
				string classIdString = section.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
				int classIdNumber = Convert.ToInt32(classIdString, System.Globalization.CultureInfo.InvariantCulture);
				result.Add(classIdNumber, section);
			}
			return result;
		}

		/// <summary>
		/// Get the paths to the dumps in order
		/// </summary>
		/// <param name="directoryPath">The directory containing the struct dumps</param>
		/// <returns>An array of absolute file paths ordered by Unity version</returns>
		/// <exception cref="ArgumentException"></exception>
		private static string[] GetOrderedFilePaths(string directoryPath)
		{
			DirectoryInfo directory = new DirectoryInfo(directoryPath);
			if (!directory.Exists)
			{
				throw new ArgumentException(nameof(directoryPath));
			}

			Dictionary<UnityVersion, string> files = new Dictionary<UnityVersion, string>();
			foreach(FileInfo file in directory.GetFiles())
			{
				if(file.Extension == DumpExtension)
				{
					string fileNameWithoutExtension = file.Name.Substring(0, file.Name.Length - DumpExtension.Length);
					UnityVersion version = UnityVersion.Parse(fileNameWithoutExtension);
					files.Add(version, file.FullName);
				}
			}
			List<UnityVersion> orderedVersions = files.Select(pair => pair.Key).ToList();
			orderedVersions.Sort();
			return orderedVersions.Select(version => files[version]).ToArray();
		}
	}
}