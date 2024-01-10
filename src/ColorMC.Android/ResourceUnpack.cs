using Android.Content;
using ColorMC.Android.components;
using ColorMC.Android.components.caciocavallo;
using ColorMC.Android.components.caciocavallo17;
using ColorMC.Android.components.lwjgl3;
using ColorMC.Android.components.security;
using ColorMC.Core.Helpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

public static class ResourceUnPack
{
    public static string ComponentsDir;
    public static void StartUnPack(Context ctx)
    {
        var dir = ctx.GetExternalFilesDir(null)!.AbsolutePath;

        ComponentsDir = dir + "/components";
        if (!Directory.Exists(ComponentsDir))
        {
            Directory.CreateDirectory(ComponentsDir);
        }

        var file = Path.GetFullPath($"{dir}/controlmap/default.json");
        if (!File.Exists(file))
        {
            PathHelper.WriteBytes(file, Resource1._default);
        }
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo/version");
        if (!File.Exists(file))
        {
            WriteCaciocavallo();
        }
        else 
        {
            var version = PathHelper.ReadText(file);
            var version1 = Encoding.UTF8.GetString(ResourceDir1.version);
            if (version != version1)
            {
                WriteCaciocavallo();
            }
        }
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/version");
        if (!File.Exists(file))
        {
            WriteCaciocavallo17();
        }
        else
        {
            var version = PathHelper.ReadText(file);
            var version1 = Encoding.UTF8.GetString(ResourceDir2.version);
            if (version != version1)
            {
                WriteCaciocavallo17();
            }
        }
        file = Path.GetFullPath($"{ComponentsDir}/lwjgl3/version");
        if (!File.Exists(file))
        {
            WriteLwjgl3();
        }
        else
        {
            var version = PathHelper.ReadText(file);
            var version1 = Encoding.UTF8.GetString(ResourceDir3.version);
            if (version != version1)
            {
                WriteLwjgl3();
            }
        }
        file = Path.GetFullPath($"{ComponentsDir}/security/version");
        if (!File.Exists(file))
        {
            WriteSecurity();
        }
        else
        {
            var version = PathHelper.ReadText(file);
            var version1 = Encoding.UTF8.GetString(ResourceDir4.version);
            if (version != version1)
            {
                WriteSecurity();
            }
        }
    }

    private static void WriteCaciocavallo()
    {
        var file = Path.GetFullPath($"{ComponentsDir}/caciocavallo/cacio-androidnw-1.10-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir1.cacio_androidnw_1_10_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo/cacio-shared-1.10-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir1.cacio_shared_1_10_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo/ResConfHack.jar");
        PathHelper.WriteBytes(file, ResourceDir1.ResConfHack);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo/version");
        PathHelper.WriteBytes(file, ResourceDir1.version);
    }

    private static void WriteCaciocavallo17()
    {
        var file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/cacio-shared-1.18-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir2.cacio_shared_1_18_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/cacio-tta-1.18-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir2.cacio_tta_1_18_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/version");
        PathHelper.WriteBytes(file, ResourceDir2.version);
    }

    private static void WriteLwjgl3()
    {
        var file = Path.GetFullPath($"{ComponentsDir}/lwjgl3/lwjgl-glfw-classes.jar");
        PathHelper.WriteBytes(file, ResourceDir3.lwjgl_glfw_classes);
        file = Path.GetFullPath($"{ComponentsDir}/lwjgl3/lwjgl-vulkan.jar");
        PathHelper.WriteBytes(file, ResourceDir3.lwjgl_vulkan);
        file = Path.GetFullPath($"{ComponentsDir}/lwjgl3/lwjgl-vulkan-native.jar");
        PathHelper.WriteBytes(file, ResourceDir3.lwjgl_vulkan_native);
        file = Path.GetFullPath($"{ComponentsDir}/lwjgl3/version");
        PathHelper.WriteBytes(file, ResourceDir3.version);
    }

    private static void WriteSecurity()
    {
        var file = Path.GetFullPath($"{ComponentsDir}/security/java_sandbox.policy");
        PathHelper.WriteBytes(file, ResourceDir4.java_sandbox);
        file = Path.GetFullPath($"{ComponentsDir}/security/log4j-rce-patch-1.12.xml");
        PathHelper.WriteBytes(file, ResourceDir4.log4j_rce_patch_1_12);
        file = Path.GetFullPath($"{ComponentsDir}/security/log4j-rce-patch-1.7.xml");
        PathHelper.WriteBytes(file, ResourceDir4.log4j_rce_patch_1_7);
        file = Path.GetFullPath($"{ComponentsDir}/security/pro-grade.jar");
        PathHelper.WriteBytes(file, ResourceDir4.pro_grade);
        file = Path.GetFullPath($"{ComponentsDir}/security/version");
        PathHelper.WriteBytes(file, ResourceDir4.version);
    }
}
