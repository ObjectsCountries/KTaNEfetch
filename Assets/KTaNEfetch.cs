using wawa.Extensions;
using wawa.Modules;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; // for detecting the OS type (Windows, Mac, Linux)
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class KTaNEfetch : ModdedModule
{

    public class SystemInfo
    {
        public Dictionary<string, object> OS;
        public string Hostname;
        public Dictionary<string, object> Kernel;
        public Dictionary<string, int> Uptime;
        public string Shell;
        public string DesktopEnvironment;
        public Dictionary<string, int>[] CPU;
        public string[] GPU;
        public Dictionary<string, object> RAM;
        public string[] LocalIP;
        public string[] SelectedComponents;
    }

    public KMSelectable[] numbers;
    public KMSelectable backspace, submit;
    public MeshRenderer logo;
    public TextMesh input;
    public TextMesh fetchText;
    private SystemInfo info;
    public Texture2D[] distros;
    public Texture2D defaultIcon;
    public KMBombInfo bomb;
    private List<string> requiredComponents = new List<string>();

    void Start()
    {
        foreach (KMSelectable numberButton in numbers)
        {
            numberButton.Set(
                        onInteract: () =>
                        {
                            if (input.text.Contains("_"))
                            {
                                input.text = ReplaceFirst(input.text, "_", numberButton.GetComponentInChildren<TextMesh>().text);
                            }
                            else
                            {
                                input.text = input.text.Substring(1) + numberButton.GetComponentInChildren<TextMesh>().text;
                            }
                        }
                    );
        }

        backspace.Set(onInteract: () =>
        {
            if (Regex.IsMatch(input.text, "\\d___"))
            {
                input.text = "____";
            }
            else if (Regex.IsMatch(input.text, "\\d\\d__"))
            {
                input.text = input.text.Substring(0, 1) + "___";
            }
            else if (Regex.IsMatch(input.text, "\\d\\d\\d_"))
            {
                input.text = input.text.Substring(0, 2) + "__";
            }
            else if (Regex.IsMatch(input.text, "\\d\\d\\d\\d"))
            {
                input.text = input.text.Substring(0, 3) + "_";
            }
        });

        submit.Set(onInteract: () =>
        {
            if (!Regex.IsMatch(input.text, "\\d\\d\\d\\d"))
            {
                Strike("STRIKE! Submitted before a full code was entered.");
                if (!Status.HasStruck)
                {
                    Log("You now must check if the CPU is from AMD.");
                }
            }
            else if (CheckComponents(input.text))
            {
                fetchText.text = PrintInfo(info);
                Solve("Code and components both correct!");
            }
        });
    }

    bool CheckComponents(string code)
    {
        requiredComponents.Clear();
        var os = Environment.OSVersion;
        Log("Trying code " + code + ".");
        try
        {
            if ((int)os.Platform == 2)
            {
                info = JsonConvert.DeserializeObject<SystemInfo>(
                        File.ReadAllText(Environment.GetEnvironmentVariable("TEMP") + "\\KTaNEfetch_" + code + ".json"));
                File.Delete(Environment.GetEnvironmentVariable("TEMP") + "\\KTaNEfetch_" + code + ".json");
            }
            else
            {
                info = JsonConvert.DeserializeObject<SystemInfo>(
                        File.ReadAllText("/tmp/KTaNEfetch_" + code + ".json"));
                File.Delete("/tmp/KTaNEfetch_" + code + ".json");
            }
        }
        catch (FileNotFoundException)
        {
            Strike("STRIKE! Incorrect code.");
            if (!Status.HasStruck)
            {
                Log("You now must check if the CPU is from AMD.");
            }
            return false;
        }

        if (info.Hostname.Length > bomb.GetModuleNames().Count)
        {
            requiredComponents.Add("Hostname");
        }

        int kernelSum = 0;
        foreach (Match digit in Regex.Matches((string)info.Kernel["release"], "\\d"))
        {
            kernelSum += int.Parse(digit.Value);
        }
        int serialSum = 0;
        foreach (int digit in bomb.GetSerialNumberNumbers())
        {
            serialSum += digit;
        }
        if (kernelSum >= serialSum)
        {
            requiredComponents.Add("Kernel");
        }

        //TODO uptime

        //TODO gameplay room (for shell)

        bool gameplayRoomIsModded = true;
        bool shellIsNotDefault = false;
        if ((int)os.Platform == 2)
        {
            Log("DEBUG: Running Windows");
            shellIsNotDefault = !info.Shell.ToLower().Contains("cmd") && !info.Shell.ToLower().Contains("powershell");
        }
        else if ((int)os.Platform == 6)
        {
            Log("DEBUG: Running MacOS");
            shellIsNotDefault = !info.Shell.ToLower().Contains("zsh");
        }
        else if ((int)os.Platform == 4)
        {
            Log("DEBUG: Running Linux");
            shellIsNotDefault = !info.Shell.ToLower().Contains("bash");
        }
        if (shellIsNotDefault || gameplayRoomIsModded)
        {
            requiredComponents.Add("Shell");
        }

        if ((int)os.Platform == 4)
        {
            Dictionary<string, string> desktopEnvironments = new Dictionary<string, string>(){
                {"budgie", "FriendshipModule"},
                {"cinnamon", "cookieJars"},
                {"deepin", "MahjongModule"},
                {"enlightenment", "GSEight"},
                {"gnome", "qkGnomishPuzzle"},
                {"kde", "modernCipher"},
                {"lumina", "TheBulbModule"},
                {"lxde", "X01"},
                {"lxqt", "notX01"},
                {"mate", "coffeebucks"},
                {"trinity", "qkTernaryConverter"},
                {"xfce", "setupWizard"},
            };
            foreach (KeyValuePair<string, string> de in desktopEnvironments)
            {
                if (info.DesktopEnvironment.ToLower().Contains(de.Key) && bomb.GetModuleIDs().Contains(de.Value))
                {
                    requiredComponents.Add("Desktop Environment");
                }
            }
        }

        bool anyAMDcpu = false;
        foreach (Dictionary<string, int> cpus in info.CPU)
        {
            foreach (KeyValuePair<string, int> cpu in cpus)
            {
                if ((cpu.Key.ToLower().Contains("intel") && !Status.HasStruck) || (cpu.Key.ToLower().Contains("amd") && Status.HasStruck))
                {
                    if (!requiredComponents.Contains("CPU"))
                    {
                        requiredComponents.Add("CPU");
                    }
                }
                if (cpu.Key.ToLower().Contains("amd"))
                {
                    anyAMDcpu = true;
                }
            }
        }

        bool anyCruel = bomb.GetModuleNames().Any(x => x.ToLower().Contains("cruel"));
        bool anyAMDgpu = info.GPU.Any(x => x.ToLower().Contains("amd"));

        if ((anyAMDcpu && anyAMDgpu) != anyCruel)
        {
            requiredComponents.Add("GPU");
        }

        if ((bomb.GetSolvedModuleNames().Count / bomb.GetSolvableModuleNames().Count * 100) <= ((double)info.RAM["used"] / (double)info.RAM["total"] * 100))
        {
            requiredComponents.Add("RAM");
        }

        //TODO add local ip
        if (code == "")
        {
            Log("Required components: " + string.Join(", ", requiredComponents.ToArray()));
        }

        bool wrong = false;

        foreach (string component in info.SelectedComponents)
        {
            if (!requiredComponents.Contains(component))
            {
                Log("Incorrectly included " + component + ".");
                wrong = true;
            }
        }

        foreach (string component in requiredComponents)
        {
            if (!info.SelectedComponents.Contains(component))
            {
                Log("Incorrectly excluded " + component + ".");
                wrong = true;
            }
        }

        if (wrong)
        {
            Strike("Incorrect components.");
        }

        return info.SelectedComponents.SequenceEqual(requiredComponents);
    }

    string PrintInfo(SystemInfo info)
    {
        logo.material.mainTexture = defaultIcon;
        foreach (Texture2D distro in distros)
        {
            if (((string)info.OS["name"]).ToLower().Contains(distro.name))
            {
                logo.material.mainTexture = distro;
            }
        }
        string result = "";
        result += "OS: " + info.OS["name"] + "\n";
        if (requiredComponents.Contains("Hostname"))
        {
            result += "Hostname: " + info.Hostname + "\n";
        }
        if (requiredComponents.Contains("Kernel"))
        {
            result += "Kernel: " + info.Kernel["name"] + " " + info.Kernel["release"] + "\n";
        }
        if (requiredComponents.Contains("Uptime"))
        {
            result += "Uptime: ";
            if (info.Uptime["days"] > 0)
            {
                result += "" + info.Uptime["days"] + " days, ";
            }
            if (info.Uptime["hours"] > 0)
            {
                result += "" + info.Uptime["hours"] + " hours, ";
            }
            if (info.Uptime["minutes"] > 0)
            {
                result += "" + info.Uptime["minutes"] + " minutes, ";
            }
            if (info.Uptime["seconds"] > 0)
            {
                result += "" + info.Uptime["seconds"] + " seconds";
            }
            result += "\n";
        }
        if (requiredComponents.Contains("Shell"))
        {
            result += "Shell: " + info.Shell + "\n";
        }
        if (requiredComponents.Contains("Desktop Environment"))
        {
            result += "Desktop Environment: " + info.DesktopEnvironment + "\n";
        }
        if (requiredComponents.Contains("CPU"))
        {
            result += "CPU: " + info.CPU[0].Keys.First() + "\n";
        }
        if (requiredComponents.Contains("GPU"))
        {
            result += "GPU: " + string.Join(",\n", info.GPU) + "\n";
        }
        if (requiredComponents.Contains("RAM"))
        {
            result += "RAM: " + (int)((double)info.RAM["used"] / (double)info.RAM["total"] * 100 + .5) + "% usage\n";
        }
        if (requiredComponents.Contains("Local IP"))
        {
            result += "Local IP: " + info.LocalIP[0] + "\n";
        }
        Log("Final output: ");
        foreach (string line in result.Split('\n'))
        {
            if (line != "")
            {
                Log(line);
            }
        }
        return result;
    }

    private string ReplaceFirst(string str, string term, string replace)
    {
        int position = str.IndexOf(term);
        if (position < 0)
        {
            return str;
        }
        str = str.Substring(0, position) + replace + str.Substring(position + term.Length);
        return str;
    }
}
