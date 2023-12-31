﻿using System.IO;
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

        public static void CreateFolderToCreatePackages()
        {
            Directory.CreateDirectory(GetBasePath());
            //refresh the assets folder
            AssetDatabase.Refresh();
        }

        public static void OpenFolderToCreatePackages()
        {
            string path = GetBasePath().Replace("\\", "/");

            OpenFolder(path);
        }

        static void OpenFolder(string path)
        {
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
            var configCreateDefaultScript = config.createDefaultScript;
            var configCreateTests = config.createTests;
            var configCreateEditor = config.createEditor;
            var configCreateGitRepo = config.createGitRepo;
            var configPackageDescription = config.packageDescription;


            Debug.Log(string.Format(PackageCreationMessage, packageName, fullPath));

            await CreateRuntimeAsmdef(packageName);
            if (configCreateEditor)
            {
                CreateEditorAsmdef(packageName);
            }

            if (configCreateTests)
            {
                CreateTestsDirectory(packageName);
            }

            await CreatePackageJson(packageName, configPackageDescription, company);

            if (configCreateGitRepo)
            {
                InitGitRepo(packageName);
            }

            if (configCreateDefaultScript)
            {
                await CreateDefaultScript(packageName);
            }

            Debug.Log(string.Format(DoneMessage, packageName, fullPath));

            if (EditorUtility.DisplayDialog("Package Created", "Would you like to open the package in file explorer?",
                    "Yes", "No"))
            {
                OpenFolder(fullPath);
            }
        }

        static async Task CreateRuntimeAsmdef(string packageName)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName);
            Directory.CreateDirectory(Path.Combine(packagePath, "Runtime"));
            await File.WriteAllTextAsync(Path.Combine(packagePath, "Runtime", $"{packageName}.Runtime.asmdef"),
                string.Format(RuntimeAsmdefTemplate, packageName));
        }

        static async void CreateEditorAsmdef(string packageName)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName);
            Directory.CreateDirectory(Path.Combine(packagePath, "Editor"));
            await File.WriteAllTextAsync(Path.Combine(packagePath, "Editor", $"{packageName}.Editor.asmdef"),
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
            var workingDirectory = Path.Combine(GetBasePath(), packageName);

            // Ejecutar git init de forma sincrónica
            Process gitInitProcess = new Process();
            gitInitProcess.StartInfo.FileName = "git";
            gitInitProcess.StartInfo.Arguments = "init";
            gitInitProcess.StartInfo.UseShellExecute = false;
            gitInitProcess.StartInfo.RedirectStandardOutput = true;
            gitInitProcess.StartInfo.WorkingDirectory = workingDirectory;
            gitInitProcess.Start();
            gitInitProcess.WaitForExit();

            // Abrir la carpeta en GitHub Desktop
            string githubDesktopUrl = $"x-github-client://openRepo/{workingDirectory}";
            Process.Start(githubDesktopUrl);
        }



        static async Task CreateDefaultScript(string packageName)
        {
            var packagePath = Path.Combine(GetBasePath(), packageName);
            var defaultScriptPath = Path.Combine(packagePath, "Runtime", $"{packageName}_DefaultScript.cs");
            await File.WriteAllTextAsync(defaultScriptPath, string.Format(DefaultScriptTemplate, packageName));
        }

        public static void DeletePackage(string packageName)
        {
            // create a path for this package name and delete folder (with all its files)
            var packagePath = Path.Combine(GetBasePath(), packageName.Replace(" ", "_"));
            Directory.Delete(packagePath, true);
            //if .meta delete too
            var packageMetaPath = packagePath + ".meta";
            if (File.Exists(packageMetaPath))
            {
                File.Delete(packageMetaPath);
            }

            //refresh the assets folder
            AssetDatabase.Refresh();
        }
    }
}