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
        public int[] Uptime;
        public string Shell;
        public string DesktopEnvironment;
        public Dictionary<string, int>[] CPU;
        public string[] GPU;
        public Dictionary<string, object> RAM;
        public string[] SelectedComponents;
    }

    public KMSelectable[] numbers;
    public KMSelectable script, instructions, backspace, submit;
    public MeshRenderer logo;
    public TextMesh input;
    public TextMesh fetchText;
    private SystemInfo info;
    public Texture2D[] distros;
    public Texture2D defaultIcon;
    public KMBombInfo bomb;
    private List<string> requiredComponents = new List<string>();
    private bool gameplayRoomIsModded;

    void Start()
    {
        gameplayRoomIsModded = !GameObject.Find("FacilityRoom");
        foreach (KMSelectable numberButton in numbers)
        {
            numberButton.Set(
                        onInteract: () =>
                        {
                            Shake(numberButton, 0.5f, Sound.BigButtonPress);
                            if (!Status.IsSolved)
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
                        }
                    );
        }

        script.Set(onInteract: () => {
            Application.OpenURL("https://github.com/ObjectsCountries/KTaNEfetch/blob/main/ktanefetch.py");
            Shake(script, 0.75f, Sound.BigButtonPress);
        });

        instructions.Set(onInteract: () => {
            Application.OpenURL("https://github.com/ObjectsCountries/KTaNEfetch/blob/main/README.md");
            Shake(instructions, 0.75f, Sound.BigButtonPress);
        });

        backspace.Set(onInteract: () =>
        {
            if (!Status.IsSolved) {
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
            }
            Shake(backspace, 0.75f, Sound.BigButtonPress);
        });

        submit.Set(onInteract: () =>
        {
            if (!Status.IsSolved) {
                if (!Regex.IsMatch(input.text, "\\d\\d\\d\\d"))
                {
                    if (!Status.HasStruck)
                    {
                        Strike("STRIKE! Submitted before a full code was entered.");
                        Log("You must now check if the CPU is from AMD.");
                    }
                    else
                    {
                        Strike("STRIKE! Submitted before a full code was entered.");
                    }
                    input.text = "____";
                }
                else if (CheckComponents(input.text))
                {
                    fetchText.text = PrintInfo(info);
                    Solve("Code and components both correct!");
                    Play(Sound.CorrectChime);
                }
            }
            Shake(submit, 0.75f, Sound.BigButtonPress);
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
            if (!Status.HasStruck)
            {
                Strike("STRIKE! Incorrect code.");
                Log("You must now check if the CPU is from AMD.");
            }
            else
            {
                Strike("STRIKE! Incorrect code.");
            }
            input.text = "____";
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

        int uptime = ((int)info.Uptime[0] * 86400) + ((int)info.Uptime[1] * 3600) + ((int)info.Uptime[2] * 60) + ((int)info.Uptime[3]);

        if (uptime < bomb.GetTime())
        {
            requiredComponents.Add("Uptime");
        }

        bool shellIsNotDefault = false;
        if ((int)os.Platform == 2)
        {
            shellIsNotDefault = !info.Shell.ToLower().Contains("cmd") && !info.Shell.ToLower().Contains("powershell");
        }
        else if ((int)os.Platform == 6)
        {
            shellIsNotDefault = !info.Shell.ToLower().Contains("zsh");
        }
        else if ((int)os.Platform == 4)
        {
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

        bool noCruel = !bomb.GetModuleNames().Any(x => x.ToLower().Contains("cruel"));
        bool anyAMDgpu = info.GPU.Any(x => x.ToLower().Contains("amd"));

        if ((anyAMDcpu && anyAMDgpu) != noCruel)
        {
            requiredComponents.Add("GPU");
        }

        if ((bomb.GetSolvedModuleNames().Count / bomb.GetSolvableModuleNames().Count * 100) <= ((double)info.RAM["used"] / (double)info.RAM["total"] * 100))
        {
            requiredComponents.Add("RAM");
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
            if (!Status.HasStruck)
            {
                Strike("STRIKE! Incorrect components.");
                Log("You must now check if the CPU is from AMD.");
            }
            else
            {
                Strike("STRIKE! Incorrect components.");
            }
            input.text = "____";
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
            List<string> uptimeResult = new List<string>();
            if (info.Uptime[0] == 1)
            {
                uptimeResult.Add("" + info.Uptime[0] + " day");
            }
            else if (info.Uptime[0] > 0)
            {
                uptimeResult.Add("" + info.Uptime[0] + " days");
            }
            if (info.Uptime[1] == 1)
            {
                uptimeResult.Add("" + info.Uptime[1] + " hour");
            }
            else if (info.Uptime[1] > 0)
            {
                uptimeResult.Add("" + info.Uptime[1] + " hours");
            }
            if (info.Uptime[2] == 1)
            {
                uptimeResult.Add("" + info.Uptime[2] + " minute");
            }
            else if (info.Uptime[2] > 0)
            {
                uptimeResult.Add("" + info.Uptime[2] + " minutes");
            }
            if (info.Uptime[3] == 1)
            {
                uptimeResult.Add("" + info.Uptime[3] + " second");
            }
            else if (info.Uptime[3] > 0)
            {
                uptimeResult.Add("" + info.Uptime[3] + " seconds");
            }
            result += "Uptime: " + String.Join(", ", uptimeResult.ToArray()) + "\n";
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
