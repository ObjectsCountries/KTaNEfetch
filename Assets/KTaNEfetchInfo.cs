using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class KTaNEfetchInfo {

    public string osName, hostname, shell, desktopEnvironment, cpu, gpu;
    public int uptimeSeconds;

    public string result;

    public KTaNEfetchInfo() {
        string result = "OS: ";
        var os = Environment.OSVersion;
        bool unix = os.Platform == PlatformID.Unix;
        bool macos = SystemInfo.operatingSystem.Contains("OS X");
        result += SystemInfo.operatingSystem + "\n";
        this.osName = SystemInfo.operatingSystem;

        string hostname = SystemInfo.deviceName;
        result += "Hostname: " + hostname + "\n";
        this.hostname = hostname;

        string shell = Environment.GetEnvironmentVariable("SHELL") ?? "CMD/Powershell";
        result += "Shell: " + shell + "\n";
        this.shell = shell;

        string desktopEnvironment = PlatformDetection(unix, macos);
        result += "Desktop Environment: " + desktopEnvironment + "\n";
        this.desktopEnvironment = desktopEnvironment;

        string cpu = GetCPU();
        result += "CPU: " + cpu + "\n";
        this.cpu = cpu;

        string gpu = GetGPU();
        result += "GPU: " + gpu + "\n";
        this.gpu = gpu;

        this.result = result;
    }

    public override string ToString() {
        return this.result;
    }

    private static string PlatformDetection(bool unix, bool macos) {
        if (unix) {
            if (macos) {
                return "Aqua";
            }
            else {
                return EnvironmentDetection();
            }
        }
        else {
            return "Metro";
        }
    }

    private static string EnvironmentDetection() {
        Dictionary<string, string> xdgDesktopNormalization = new Dictionary<string, string>(){
            { "DDE", "Deepin" },
            { "ENLIGHTENMENT", "Enlightenment" },
            { "GNOME-CLASSIC", "GNOME" },
            { "GNOME-FLASHBACK", "GNOME" },
            { "TDE", "Trinity" },
            { "X-CINNAMON", "Cinnamon" }
        };
        string desktopIdentifiers = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "Unknown DE";
        string[] DIparts = desktopIdentifiers.Split(':');
        if (xdgDesktopNormalization.ContainsKey(DIparts[0].ToUpper())) {
            return xdgDesktopNormalization[DIparts[0].ToUpper()];
        }
        string gnome = Environment.GetEnvironmentVariable("GNOME_DESKTOP_SESSION_ID") ?? "";
        string hyprland = Environment.GetEnvironmentVariable("HYPRLAND_CMD") ?? "";
        string kde = Environment.GetEnvironmentVariable("KDE_FULL_SESSION") ?? "";
        string mate = Environment.GetEnvironmentVariable("MATE_DESKTOP_SESSION_ID") ?? "";
        string trinity = Environment.GetEnvironmentVariable("TDE_FULL_SESSION") ?? "";
        if (gnome != "") {
            return "GNOME";
        }
        if (hyprland != "") {
            return "Hyprland";
        }
        if (kde != "") {
            return "KDE";
        }
        if (mate != "") {
            return "MATE";
        }
        if (trinity != "") {
            return "Trinity";
        }

        Dictionary<string, string> deNormalization = new Dictionary<string, string>(){
            { "budgie-desktop", "Budgie" },
            { "cinnamon", "Cinnamon" },
            { "deepin", "Deepin" },
            { "enlightenment", "Enlightenment" },
            { "gnome", "Gnome" },
            { "kde", "KDE" },
            { "lumina", "Lumina" },
            { "lxde", "LXDE" },
            { "lxqt", "LXQt" },
            { "mate", "MATE" },
            { "muffin", "Cinnamon" },
            { "trinity", "Trinity" },
            { "xfce session", "Xfce" },
            { "xfce", "Xfce" },
            { "xfce4", "Xfce" },
            { "xfce5", "Xfce" }
        };
        string legacyDEidentifier = Environment.GetEnvironmentVariable("DE") ?? "Unknown DE";
        if (deNormalization.ContainsKey(legacyDEidentifier.ToLower())) {
            return deNormalization[legacyDEidentifier.ToLower()];
        }

        return "Unknown DE";
    }

    private static string GetCPU() {
        return SystemInfo.processorType;
    }

    private static string GetGPU() {
        return SystemInfo.graphicsDeviceName;
    }
}
