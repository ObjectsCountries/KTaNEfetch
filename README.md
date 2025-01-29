# KTaNEfetch

## How to Use `ktanefetch.py`

1. Install [Python](https://python.org/downloads).
2. Download [`ktanefetch.py`](ktanefetch.py).
3. Run the following command in a terminal: `python3 -m pip install archey4 questionary`
4. Run the downloaded script in a terminal.

In case that doesn't work...

1. Create a new folder of any name.
2. Within that folder, create a folder called `src`.
3. Put `ktanefetch.py` in the `src` folder.
4. Open a terminal within the folder just outside of `src`.
5. Run the following commands (note that there must be a space after `venv` for the first command):

```
python3 -m venv .
```

If on Windows: (Note: run `Activate.ps1` if using Powershell.)

```
cd Scripts
activate
```

If on Mac/Linux: (Note: Run `source activate.fish` if using fish, and `source activate.csh` if using csh.)

```
cd bin
source activate
```

Then:

```
cd ..
pip install archey4 questionary
python3 src/ktanefetch.py
```
