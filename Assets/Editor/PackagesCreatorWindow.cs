using UnityEditor;
using System.IO;
using UnityEngine;

namespace Editor
{
    public class PackagesCreatorWindow : EditorWindow
    {
        private string packageName
        {
            get => config.packageName;
            set => config.packageName = value;
        }

        readonly PackageCreationConfig config = new();


        [MenuItem("Packages/Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<PackagesCreatorWindow>(false, "Packages Creator");
            window.Show();
        }


        void OnGUI()
        {
            if (!PackagesCreator.ExistsFolderToCreatePackages())
            {
                if (GUILayout.Button("Create Packages Folder"))
                {
                    PackagesCreator.CreateFolderToCreatePackages();
                }

                return;
            }
            
            if (GUILayout.Button("Open Packages Folder"))
            {
                PackagesCreator.OpenFolderToCreatePackages();
            }


            WriteProperties();

            if (!IsValidated(config))
            {
                EditorGUILayout.HelpBox("Please correct all the errors on the fields", MessageType.Warning);
                return;
            }

            if (PackagesCreator.PackageExists(packageName))
            {
                EditorGUILayout.HelpBox(
                    $"A package with the name {packageName} already exists. Please choose a different name.",
                    MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Create Package"))
            {
                {
                    PackagesCreator.CreatePackage(new PackageCreationConfig
                    {
                        packageName = packageName,
                        // Provide other necessary properties here
                    });
                    Debug.Log($"Package {packageName} created successfully!");
                }
            }
        }

        void WriteProperties()
        {
            GUILayout.Label("Create a new Unity package", EditorStyles.boldLabel);
            GUILayout.Space(2);
            packageName = EditorGUILayout.TextField("Package Name", packageName);
            config.createTests = EditorGUILayout.Toggle("Create Tests", config.createTests);
            config.createEditor = EditorGUILayout.Toggle("Create Editor", config.createEditor);
            config.createDefaultScript = EditorGUILayout.Toggle("Create Default Script", config.createDefaultScript);
            config.createGitRepo = EditorGUILayout.Toggle("Create Git Repo", config.createGitRepo);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Name");
            if (!PackageConfigValidator.ValidatePackageName(config.packageName, out string nameErrorMessage))
            {
                EditorGUILayout.HelpBox(nameErrorMessage, MessageType.Warning);
            }

            config.packageName = EditorGUILayout.TextField(config.packageName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Description");
            if (!PackageConfigValidator.ValidatePackageDescription(config.packageDescription,
                    out string descriptionErrorMessage))
            {
                EditorGUILayout.HelpBox(descriptionErrorMessage, MessageType.Warning);
            }

            config.packageDescription = EditorGUILayout.TextField(config.packageDescription);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Author");
            if (!PackageConfigValidator.ValidatePackageAuthor(config.packageAuthor, out string authorErrorMessage))
            {
                EditorGUILayout.HelpBox(authorErrorMessage, MessageType.Warning);
            }

            config.packageAuthor = EditorGUILayout.TextField(config.packageAuthor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Author Email");
            if (!PackageConfigValidator.ValidatePackageAuthorEmail(config.packageAuthorEmail,
                    out string emailErrorMessage))
            {
                EditorGUILayout.HelpBox(emailErrorMessage, MessageType.Warning);
            }

            config.packageAuthorEmail = EditorGUILayout.TextField(config.packageAuthorEmail);
            EditorGUILayout.EndHorizontal();
        }

        public static bool IsValidated(PackageCreationConfig config)
        {
            bool isValid = true;
            string errorMessage = string.Empty;

            // Validate Package Name
            if (!PackageConfigValidator.ValidatePackageName(config.packageName, out errorMessage))
            {
                isValid = false;
                Debug.LogWarning(errorMessage);
            }

            // Validate Package Description
            if (!PackageConfigValidator.ValidatePackageDescription(config.packageDescription, out errorMessage))
            {
                isValid = false;
                Debug.LogWarning(errorMessage);
            }

            // Validate Package Author
            if (!PackageConfigValidator.ValidatePackageAuthor(config.packageAuthor, out errorMessage))
            {
                isValid = false;
                Debug.LogWarning(errorMessage);
            }

            // Validate Package Author Email
            if (!PackageConfigValidator.ValidatePackageAuthorEmail(config.packageAuthorEmail, out errorMessage))
            {
                isValid = false;
                Debug.LogWarning(errorMessage);
            }

            return isValid;
        }
    }
}