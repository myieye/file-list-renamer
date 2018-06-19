using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace FileListRenamer
{
  class Program
  {
    const string DEFAULT_PATH = "./";
    const string DEFAULT_NAMES_FILE = DEFAULT_PATH + "names.txt";
    const string DEFAULT_PATTERN = "";
    const string RENAMED_FOLDER = "renamed";
    const string HELP_FLAG = "-help";

    static void Main(string[] args)
    {
      if (!ArgsAreValid(args)) return;
      if (CheckForHelpArg(args)) return;

      string path = GetTargetPath(args);
      string namesFilePath = Path.GetFullPath(GetNamesFilePath(args));
      string pattern = GetPattern(args);

      // Check names file
      if (!CheckFileExists(namesFilePath, "Names file not found.")) return;
      String[] names = File.ReadAllLines(namesFilePath);

      // Find and check files to rename
      DirectoryInfo d = new DirectoryInfo(path);
      FileInfo[] files = d.GetFiles();
      var sortedFiles = files.Where(file =>
        !SamePath(file.FullName, namesFilePath) &&
        !SamePath(file.FullName, System.Reflection.Assembly.GetEntryAssembly().Location) &&
        Regex.IsMatch(file.Name, pattern))
        .OrderBy(f => f.Name);
      if (!CheckEqual(sortedFiles.Count(), names.Count(),
        string.Format("Count mismatch. Found {0} files and {1} file names.", sortedFiles.Count(), names.Count()))) return;

      // Prepare rename directory
      string renamePath = Path.Combine(path, RENAMED_FOLDER);
      CreateDirectoryIfNotExists(renamePath);

      // Check for overwriting
      int overwriteCount = 0;
      int i = 0;
      foreach (FileInfo file in sortedFiles)
      {
        var newName = names[i++];
        var newFileName = Path.HasExtension(newName) ? newName : newName + file.Extension;
        if (File.Exists(Path.Combine(renamePath, newFileName)))
        {
          overwriteCount++;
        }
      }
      if (!CheckIsZero(overwriteCount, string.Format("Operation would overwrite {0} files.", overwriteCount))) return;

      // Copy renamed files
      i = 0;
      foreach (FileInfo file in sortedFiles)
      {
        var newName = names[i++];
        var newFileName = Path.HasExtension(newName) ? newName : newName + file.Extension;
        var newPath = Path.Combine(renamePath, newFileName);
        File.Copy(file.FullName, newPath);
        Console.WriteLine("Renaming: {0} to {1}", file.Name, newPath);
      }
    }

    private static void CreateDirectoryIfNotExists(string path)
    {
      if (!Directory.Exists(path))
      {
        Console.WriteLine("Creating directory: " + path);
        Directory.CreateDirectory(path);
      }
    }

    private static bool CheckIsZero(int count, string errorMsg)
    {
      return CheckTrue(count == 0, errorMsg);
    }

    private static bool CheckFileExists(string path, string errorMsg)
    {
      return CheckTrue(File.Exists(path), errorMsg);
    }

    private static bool CheckEqual(int value1, int value2, string errorMsg)
    {
      return CheckTrue(value1 == value2, errorMsg);
    }

    private static bool CheckTrue(bool result, string errorMsg)
    {
      if (!result)
      {
        Console.WriteLine("Error: " + errorMsg);
        PrintHelp();
        return false;
      }
      return true;
    }

    private static bool SamePath(string path1, string path2)
    {
      return path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool CheckForHelpArg(string[] args)
    {
      if (args.Length >= 1 && args[0].Equals(HELP_FLAG, StringComparison.InvariantCultureIgnoreCase))
      {
        PrintHelp();
        return true;
      }
      return false;
    }

    private static bool ArgsAreValid(string[] args)
    {
      if (args.Length > 3)
      {
        Console.WriteLine("Invalid arguments\r\n");
        PrintHelp();
        return false;
      }
      return true;
    }

    private static void PrintHelp()
    {
      Console.WriteLine();
      Console.WriteLine("File List Renamer:");
      Console.WriteLine("Renames a directory of files given a list of new names.");
      Console.WriteLine();
      Console.WriteLine("Possible parameters:");
      Console.WriteLine("[<path-to-targeted-folder> [<path-to-names-file> [<search-pattern>]]]");
      Console.WriteLine();
      Console.WriteLine("Default parameters:");
      Console.WriteLine("{0} {1} \"{2}\"", DEFAULT_PATH, DEFAULT_NAMES_FILE, DEFAULT_PATTERN);
      Console.WriteLine();
      Console.WriteLine("Use {0} for this help info.", HELP_FLAG);
      Console.WriteLine("New files are placed in the folder <path-to-targeted-folder>/{0}/.", RENAMED_FOLDER);
      Console.WriteLine("The tool will ignore itself and the names file, if these are in the targeted folder.");
      Console.WriteLine();
    }

    private static string GetTargetPath(string[] args)
    {
      return GetIndexOrValue(args, 0, DEFAULT_PATH);
    }

    private static string GetNamesFilePath(string[] args)
    {
      return GetIndexOrValue(args, 1, DEFAULT_NAMES_FILE);
    }

    private static string GetPattern(string[] args)
    {
      return GetIndexOrValue(args, 2, DEFAULT_PATTERN);
    }

    private static string GetIndexOrValue(string[] args, int i, string value)
    {
      return args.Length > i ? args[i] : value;
    }
  }
}
