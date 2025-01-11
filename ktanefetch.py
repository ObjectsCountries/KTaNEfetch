#!/usr/bin/env python3

from json import dump
import os
import platform
from random import randint
from typing import Any

from archey.entries import (
    cpu,
    desktop_environment,
    distro,
    gpu,
    hostname,
    kernel,
    ram,
    shell,
    uptime,
)
from questionary import checkbox


class KTaNEfetch:
    def __init__(self) -> None:
        self._result: dict[str, Any] = {}
        self._code: int = 0
        self._choices: list[str] = []
        self._COMPONENTS: list[str] = [
            "Hostname",
            "Kernel",
            "Uptime",
            "Shell",
            "Desktop Environment",
            "CPU",
            "GPU",
            "RAM",
        ]

    def run(self) -> None:
        print("Hostname: " + hostname.Hostname().value)
        print("Kernel: " + kernel.Kernel().value["release"])
        uptime_string: str = ""
        uptime_dict: dict[str, int] = uptime.Uptime().value
        if uptime_dict["days"] > 0:
            uptime_string += str(uptime_dict["days"]) + " days, "
        if uptime_dict["hours"] > 0:
            uptime_string += str(uptime_dict["hours"]) + " hours, "
        if uptime_dict["minutes"] > 0:
            uptime_string += str(uptime_dict["minutes"]) + " minutes, "
        if uptime_dict["seconds"] >= 0:
            uptime_string += str(uptime_dict["seconds"]) + " seconds"
        print("Uptime: " + uptime_string)
        print("Shell: " + shell.Shell().value)
        print("Desktop Environment: " + desktop_environment.DesktopEnvironment().value)
        print("CPU: " + str(cpu.CPU().value[0]))
        print("GPU: " + str(gpu.GPU().value))
        print(
            "RAM Usage: "
            + str(int(ram.RAM().value["used"] / ram.RAM().value["total"] * 100 + 0.5))
            + "%"
        )
        self._choices = checkbox(
            "Select components",
            choices=self._COMPONENTS,
            use_arrow_keys=True,
            use_jk_keys=True,
        ).ask()
        self._code = randint(0, 9999)
        print(f"Chosen components: {', '.join(self._choices)}")
        print(f"Your code is {self._code:04}.")
        self._result["OS"] = distro.Distro().value
        for component in self._COMPONENTS:
            match component:
                case "Hostname":
                    self._result["Hostname"] = hostname.Hostname().value
                case "Kernel":
                    self._result["Kernel"] = kernel.Kernel().value
                case "Uptime":
                    self._result["Uptime"] = uptime.Uptime().value
                case "Shell":
                    self._result["Shell"] = shell.Shell().value
                case "Desktop Environment":
                    self._result["DesktopEnvironment"] = (
                        desktop_environment.DesktopEnvironment().value
                    )
                case "CPU":
                    self._result["CPU"] = cpu.CPU().value
                case "GPU":
                    self._result["GPU"] = gpu.GPU().value
                case "RAM":
                    self._result["RAM"] = ram.RAM().value
        self._result["SelectedComponents"] = self._choices
        if platform.system() == "Windows":
            with open(
                f"{os.environ.get('TEMP')}\\KTaNEfetch_{self._code:04}.json", "a"
            ) as file:
                dump(self._result, file, indent=4)
        else:
            with open(f"/tmp/KTaNEfetch_{self._code:04}.json", "a") as file:
                dump(self._result, file, indent=4)
        if "Uptime" in self._choices:
            uptime_string_new: str = ""
            if uptime_dict["days"] > 0:
                uptime_string_new += str(self._result["Uptime"]["days"]) + " days, "
            if uptime_dict["hours"] > 0:
                uptime_string_new += str(self._result["Uptime"]["hours"]) + " hours, "
            if uptime_dict["minutes"] > 0:
                uptime_string_new += (
                    str(self._result["Uptime"]["minutes"]) + " minutes, "
                )
            if uptime_dict["seconds"] >= 0:
                uptime_string_new += str(self._result["Uptime"]["seconds"]) + " seconds"
            print("Uptime in output: " + uptime_string_new)
        if "RAM" in self._choices:
            print(
                "RAM in output: "
                + str(
                    int(
                        self._result["RAM"]["used"] / self._result["RAM"]["total"] * 100
                        + 0.5
                    )
                )
                + "%"
            )
        _ = input("Press Enter to finish the program.\n")


if __name__ == "__main__":
    KTaNEfetch().run()
