using System.Text.Json;
using Crux.Graphics;
using System.Reflection;
using StbImageSharp;
using OpenTK.Windowing.Common.Input;

namespace Crux.Utilities.IO;

public static class AssetHandler
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
        {
            new QuaternionJsonConverter(),
            new Vector3JsonConverter(),
            new Vector2JsonConverter(),
            new Color4JsonConverter()
        }
    };

    public static string GameAssetPath = "Game/Assets";

    public static string GetShortInfo()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Unique Full Meshes - {ObjHandler.UniqueMeshes.Count}x");

        return sb.ToString();
    }
    
    public static void ListAllEmbeddedResources()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Console.WriteLine(assembly.GetManifestResourceNames().Length);
        Console.WriteLine("Assembly Location: " + assembly.Location);

        // Get all embedded resource names
        string[] resourceNames = assembly.GetManifestResourceNames();

        // Output all resource names
        Console.WriteLine("Embedded resources in the assembly:");
        foreach (var resource in resourceNames)
        {
            Console.WriteLine(resource);
        }
    }

    public static Stream GetStream(string path)
    {
        if(string.IsNullOrEmpty(path))
            return null;
        
        path = path.Replace("/", "\\");
        Assembly assembly = Assembly.GetExecutingAssembly();

        Stream stream = assembly.GetManifestResourceStream(path);
        if (stream == null)
        {
            Logger.LogWarning($"Asset '{path}' not found.");
            return null;
        }
        return stream;
    }

    public static bool AssetExists(string path)
    {
        return GetStream(path) != null;
    }

    public static string ReadAssetInFull(string path)
    {
        using (var stream = GetStream(path))
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public static WindowIcon LoadIcon()
    {
        string path = "Crux/Assets/logo.png";
        
        using (Stream stream = GetStream(path)) 
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            byte[] pixelData = image.Data;
            Image iconImage = new Image(image.Width, image.Height, pixelData);
            
            return new WindowIcon(iconImage);
        }
    }

    public static int IterateBuildNumber()
    {
        string path = "Crux/Assets/history.json";
        Dictionary<string, int> history = [];
        int buildNumber = 1;

        if(AssetExists(path))
        {
            string data = ReadAssetInFull(path);
            history = JsonSerializer.Deserialize<Dictionary<string, int>>(data) ?? [];

            if (history.TryGetValue(GameEngine.Version.ToString(), out buildNumber))
            {
                if(GameEngine.InDebugMode())
                    buildNumber++;
                history[GameEngine.Version.ToString()] = buildNumber;
            }
            else
            {
                history.Add(GameEngine.Version.ToString(), buildNumber);
            }

        }else
        {
            history.Add(GameEngine.Version.ToString(), buildNumber);
        }

        if(GameEngine.InDebugMode())
            File.WriteAllText(path, JsonSerializer.Serialize(history, JsonOptions));

        return buildNumber;
    }

    public enum ShaderPresets
    {
        Lit,
        Instance_Lit,
        Font,
        Outline,
        Skybox,
    }

    public static Shader LoadPresetShader(ShaderPresets shaderPreset, string texturePath = "")
    {    
        switch (shaderPreset)
        {
            case ShaderPresets.Lit:
                return new Shader
                (
                    "Crux/Assets/Shaders/Required/Vertex/vert_lit.glsl", 
                    "Crux/Assets/Shaders/Required/Fragment/frag_lit.glsl",
                    AssetExists(texturePath) ? texturePath : "Crux/Assets/Textures/Required/Missing.jpg"
                );
            case ShaderPresets.Instance_Lit:
                return new Shader
                (
                    "Crux/Assets/Shaders/Required/Vertex/instance_vert_lit.glsl", 
                    "Crux/Assets/Shaders/Required/Fragment/frag_lit.glsl",
                    AssetExists(texturePath) ? texturePath : "Crux/Assets/Textures/Required/Missing.jpg"
                );
            case ShaderPresets.Font:
                return new Shader
                (
                    "Crux/Assets/Shaders/Required/Vertex/vert_font.glsl", 
                    "Crux/Assets/Shaders/Required/Fragment/frag_font.glsl",
                    AssetExists(texturePath) ? texturePath : "Crux/Assets/Fonts/PublicSans.jpg"
                );
            case ShaderPresets.Outline:
                return new Shader
                (
                    "Crux/Assets/Shaders/Required/Vertex/vert_lit.glsl", 
                    "Crux/Assets/Shaders/Required/Fragment/frag_outline.glsl",
                    ""
                );
            case ShaderPresets.Skybox:
                return new Shader
                (
                    "Crux/Assets/Shaders/Required/Vertex/vert_skybox.glsl", 
                    "Crux/Assets/Shaders/Required/Fragment/frag_skybox.glsl",
                    ""
                );
        }

        return null!;
    }
}
