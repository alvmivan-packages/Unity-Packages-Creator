using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    public class PackageCreationConfig
    {
        public bool createTests = true;
        public bool createEditor = true;
        public bool createDefaultScript = false;
        public bool createGitRepo = true;

        public string packageName = "MyCoolPackage";
        public string packageDescription = "";
        public string packageAuthor = "Marcos Alvarez";
        public string packageAuthorEmail = "alvmivan@gmail.com";
        public string organization = "orbitar";
    }

    public class PackageConfigValidator
    {
        public static bool ValidatePackageName(string packageName, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(packageName))
            {
                errorMessage = "Package name cannot be empty or whitespace.";
                return false;
            }

            if (packageName.Contains(" "))
            {
                errorMessage = "Package name cannot contain spaces.";
                return false;
            }

            return true;
        }

        public static bool ValidatePackageDescription(string packageDescription, out string errorMessage)
        {
            // No specific validation rules for package description
            errorMessage = null;
            return true;
        }

        public static bool ValidatePackageAuthor(string packageAuthor, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(packageAuthor))
            {
                errorMessage = "Package author cannot be empty or whitespace.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public static bool ValidatePackageAuthorEmail(string packageAuthorEmail, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(packageAuthorEmail))
            {
                errorMessage = "Package author email cannot be empty or whitespace.";
                return false;
            }

            // You can add additional email validation logic here if needed

            errorMessage = null;
            return true;
        }

        public static bool ValidateOrganization(string organization, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(organization))
            {
                errorMessage = "Organization cannot be empty or whitespace.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }



    public class PackagesCreator
    {
        const string PackageCreationMessage = "Creating package {0} at {1}";
        const string DoneMessage = "Done! Package {0} created at {1}";

        const string PackageJsonTemplate = @"{{
            ""name"": ""com.{0}.{1}"",
            ""version"": ""0.0.1"",
            ""displayName"": ""{2}"",
            ""description"": ""{3}"",
            ""unity"": ""2021.3"",
            ""dependencies"": {{}}
        }}";

        const string RuntimeAsmdefTemplate = @"{{
            ""name"": ""{0}.Runtime"",
            ""references"": [],
            ""includePlatforms"": [],
            ""excludePlatforms"": [],
            ""allowUnsafeCode"": false,
            ""overrideReferences"": false,
            ""precompiledReferences"": [],
            ""autoReferenced"": true,
            ""defineConstraints"": [],
            ""versionDefines"": [],
            ""noEngineReferences"": false
        }}";

        const string EditorAsmdefTemplate = @"{{
            ""name"": ""{0}.Editor"",
            ""references"": [""{0}.Runtime""],
            ""includePlatforms"": [""Editor""],
            ""excludePlatforms"": [],
            ""allowUnsafeCode"": false,
            ""overrideReferences"": false,
            ""precompiledReferences"": [],
            ""autoReferenced"": true,
            ""defineConstraints"": [],
            ""versionDefines"": [],
            ""noEngineReferences"": false
        }}";

        const string DefaultScriptTemplate = @"using UnityEngine;

        namespace {0}
        {{
            public class {0}_DefaultScript : MonoBehaviour
            {{
                void Start()
                {{
                    Debug.Log(""Hello World! this is the package {0}"");
                }}
            }}
        }}";
        
        
        public const string FolderToCreatePackages = "MyPackages";

        public static bool ExistsFolderToCreatePackages()
        {
            return Directory.Exists(GetBasePath());
        }
        
        public static  void CreateFolderToCreatePackages()
        {
            Directory.CreateDirectory(GetBasePath());
            //refresh the assets folder
            AssetDatabase.Refresh();
        }
        
        public static void OpenFolderToCreatePackages()
        {
            string path = GetBasePath().Replace("\\", "/");
    
            // Validar el path para asegurarse de que se abra correctamente en el explorador de archivos
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = "\"" + path + "\""; // Agregar comillas para manejar espacios en el path en Windows
            }
    
            Process.Start(path);
        }

        
        static string GetBasePath()
        {
            //"(abs)/Assets/Packages/"
            // inside the folder Assets create a folder called FolderToCreatePackages
            return Path.Combine(Application.dataPath, FolderToCreatePackages);
        }

        public static bool PackageExists(string packageName)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName.Replace(" ", "_"));
            return Directory.Exists(packagePath);
        }

        public static async void CreatePackage(PackageCreationConfig config)
        {
            var packageName = config.packageName.Replace(" ", "_");
            var company = config.organization.Replace(" ", "_").ToLower();
            var basePath = GetBasePath();
            var fullPath = Path.Combine(basePath, packageName);

            Debug.Log(string.Format(PackageCreationMessage, packageName, fullPath));

            await CreateRuntimeAsmdef(packageName);
            if (config.createEditor)
            {
                CreateEditorAsmdef(packageName);
            }

            if (config.createTests)
            {
                CreateTestsDirectory(packageName);
            }

            await CreatePackageJson(packageName, config.packageDescription, company);

            if (config.createGitRepo)
            {
                InitGitRepo(packageName);
            }

            if (config.createDefaultScript)
            {
                await CreateDefaultScript(packageName);
            }

            Debug.Log(string.Format(DoneMessage, packageName, fullPath));

            if (EditorUtility.DisplayDialog("Package Created", "Would you like to open the package in file explorer?",
                    "Yes", "No"))
            {
                Process.Start("explorer.exe", fullPath);
            }
        }

        static async Task CreateRuntimeAsmdef(string packageName)
        {
            Directory.CreateDirectory(Path.Combine(packageName, "Runtime"));
            await File.WriteAllTextAsync(Path.Combine(packageName, "Runtime", $"{packageName}.Runtime.asmdef"),
                string.Format(RuntimeAsmdefTemplate, packageName));
        }

        static async void CreateEditorAsmdef(string packageName)
        {
            var editorPath = Path.Combine(packageName, "Editor");
            Directory.CreateDirectory(Path.Combine(packageName, "Editor"));
            await File.WriteAllTextAsync(Path.Combine(editorPath, $"{packageName}.Editor.asmdef"),
                string.Format(EditorAsmdefTemplate, packageName));
        }

        static void CreateTestsDirectory(string packageName)
        {
            Directory.CreateDirectory(Path.Combine(GetBasePath(), packageName, "Tests"));
        }

        static async Task CreatePackageJson(string packageName, string packageDescription, string company)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName);
            await File.WriteAllTextAsync(Path.Combine(packagePath, "package.json"),
                string.Format(PackageJsonTemplate, company, packageName.ToLower(), packageName, packageDescription));
        }

        static void InitGitRepo(string packageName)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"init {packageName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        static async Task CreateDefaultScript(string packageName)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName);

            await File.WriteAllTextAsync(Path.Combine(packagePath, "Runtime", $"{packageName}_DefaultScript.cs"),
                string.Format(DefaultScriptTemplate, packageName));
        }
    }
}