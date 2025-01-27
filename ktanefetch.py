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
        self._code: int = 0
        self._choices: list[str] = []
        uptime_output_list: list[str] = []
        operating_system = distro.Distro().value
        device_hostname = hostname.Hostname().value
        kernel_info = kernel.Kernel().value
        uptime_dict = uptime.Uptime().value
        cmd_shell = shell.Shell().value
        de = desktop_environment.DesktopEnvironment().value
        cpus = cpu.CPU().value
        gpus = gpu.GPU().value
        memory = ram.RAM().value
        uptime_list: list[int] = [
            uptime_dict["days"],
            uptime_dict["hours"],
            uptime_dict["minutes"],
            uptime_dict["seconds"],
        ]
        if uptime_dict["days"] == 1:
            uptime_output_list.append(str(uptime_dict["days"]) + " day")
        elif uptime_dict["days"] > 0:
            uptime_output_list.append(str(uptime_dict["days"]) + " days")
        if uptime_dict["hours"] == 1:
            uptime_output_list.append(str(uptime_dict["hours"]) + " hour")
        elif uptime_dict["hours"] > 0:
            uptime_output_list.append(str(uptime_dict["hours"]) + " hours")
        if uptime_dict["minutes"] == 1:
            uptime_output_list.append(str(uptime_dict["minutes"]) + " minute")
        elif uptime_dict["minutes"] > 0:
            uptime_output_list.append(str(uptime_dict["minutes"]) + " minutes")
        if uptime_dict["seconds"] == 1:
            uptime_output_list.append(str(uptime_dict["seconds"]) + " second")
        elif uptime_dict["seconds"] > 0:
            uptime_output_list.append(str(uptime_dict["seconds"]) + " seconds")
        self._result: dict[str, Any] = {
            "OS": operating_system,
            "Hostname": device_hostname,
            "Kernel": kernel_info,
            "Uptime": uptime_list,
            "Shell": cmd_shell,
            "DesktopEnvironment": de,
            "CPU": cpus,
            "GPU": gpus,
            "RAM": memory,
        }
        self._output: dict[str, str] = {
            "OS": operating_system["name"],
            "Hostname": device_hostname,
            "Kernel": kernel_info["name"] + " " + kernel_info["release"],
            "Uptime": ", ".join(uptime_output_list),
            "Shell": cmd_shell,
            "Desktop Environment": de,
            "CPU": ", ".join(list(list(cpu.keys())[0] for cpu in cpus)),
            "GPU": ", ".join(gpus),
            "RAM": str(
                int(100 * memory["used"] / memory["total"] + 0.5)
            )
            + "%",
        }

    def run(self) -> None:
        for component, value in self._output.items():
            print(f"{component}: {str(value)}")
        self._choices = checkbox(
            "Select components",
            choices=list(self._output.keys())[1:],
            use_arrow_keys=True,
            use_jk_keys=True,
        ).ask()
        self._code = randint(0, 9999)
        print(f"Chosen components: {', '.join(self._choices)}")
        print(f"Your code is {self._code:04}.")
        self._result["SelectedComponents"] = self._choices
        if platform.system() == "Windows":
            with open(
                f"{os.environ.get('TEMP')}\\KTaNEfetch_{self._code:04}.json", "xt"
            ) as file:
                dump(self._result, file, indent=4)
        else:
            with open(f"/tmp/KTaNEfetch_{self._code:04}.json", "xt") as file:
                dump(self._result, file, indent=4)
        _ = input("Press Enter to finish the program.\n")


if __name__ == "__main__":
    KTaNEfetch().run()
