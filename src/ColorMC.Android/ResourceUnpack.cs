using Android.Content;
using Android.Util;
using ColorMC.Android.components;
using ColorMC.Android.components.caciocavallo;
using ColorMC.Android.components.caciocavallo17;
using ColorMC.Android.components.lwjgl3;
using ColorMC.Android.components.security;
using ColorMC.Core.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        Log.Info("Unpack", "Unpack Caciocavallo");
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
        Log.Info("Unpack", "Unpack Caciocavallo17");
        var file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/cacio-shared-1.18-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir2.cacio_shared_1_18_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/cacio-tta-1.18-SNAPSHOT.jar");
        PathHelper.WriteBytes(file, ResourceDir2.cacio_tta_1_18_SNAPSHOT);
        file = Path.GetFullPath($"{ComponentsDir}/caciocavallo17/version");
        PathHelper.WriteBytes(file, ResourceDir2.version);
    }

    private static void WriteLwjgl3()
    {
        Log.Info("Unpack", "Unpack Lwjgl3");
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
        Log.Info("Unpack", "Unpack Security");
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

    public static void GetCacioJavaArgs(List<string> args, int width, int height, bool java8)
    {
        // Caciocavallo config AWT-enabled version
        args.Add("-Djava.awt.headless=false");
        args.Add("-Dcacio.managed.screensize=" + width + "x" + height);
        args.Add("-Dcacio.font.fontmanager=sun.awt.X11FontManager");
        args.Add("-Dcacio.font.fontscaler=sun.font.FreetypeFontScaler");
        args.Add("-Dswing.defaultlaf=javax.swing.plaf.metal.MetalLookAndFeel");
        if (java8)
        {
            args.Add("-Dawt.toolkit=net.java.openjdk.cacio.ctc.CTCToolkit");
            args.Add("-Djava.awt.graphicsenv=net.java.openjdk.cacio.ctc.CTCGraphicsEnvironment");
        }
        else
        {
            args.Add("-Dawt.toolkit=com.github.caciocavallosilano.cacio.ctc.CTCToolkit");
            args.Add("-Djava.awt.graphicsenv=com.github.caciocavallosilano.cacio.ctc.CTCGraphicsEnvironment");
            args.Add("-Djava.system.class.loader=com.github.caciocavallosilano.cacio.ctc.CTCPreloadClassLoader");

            args.Add("--add-exports=java.desktop/java.awt=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/java.awt.peer=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.awt.image=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.java2d=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/java.awt.dnd.peer=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.awt=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.awt.event=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.awt.datatransfer=ALL-UNNAMED");
            args.Add("--add-exports=java.desktop/sun.font=ALL-UNNAMED");
            args.Add("--add-exports=java.base/sun.security.action=ALL-UNNAMED");
            args.Add("--add-opens=java.base/java.util=ALL-UNNAMED");
            args.Add("--add-opens=java.desktop/java.awt=ALL-UNNAMED");
            args.Add("--add-opens=java.desktop/sun.font=ALL-UNNAMED");
            args.Add("--add-opens=java.desktop/sun.java2d=ALL-UNNAMED");
            args.Add("--add-opens=java.base/java.lang.reflect=ALL-UNNAMED");

            // Opens the java.net package to Arc DNS injector on Java 9+
            args.Add("--add-opens=java.base/java.net=ALL-UNNAMED");
        }

        var cacioClasspath = new StringBuilder();
        cacioClasspath.Append("-Xbootclasspath/").Append(java8 ? "p" : "a");
        var cacioFiles = Directory.GetFiles(ComponentsDir + "/caciocavallo" + (java8 ? "" : "17"));
        if (cacioFiles != null)
        {
            foreach (var file in cacioFiles)
            {
                if (file.EndsWith(".jar"))
                {
                    cacioClasspath.Append(':').Append(file);
                }
            }
        }
        args.Add(cacioClasspath.ToString());
    }
}
