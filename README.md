# umpatcher update for Unity versions 2021.xx 
*by Neoshrimp*

Mono project structure seems to have changed quite a bit going from 2020.xx to 2021.xx. So much so that the current umpacher is unable to handle it. This fork attempts to fix that.

Main changes:
- Most of the git actions and checks for target repo (dnSpy-mono) were removed.
- Added missing dependencies (libatomic_ops, brotli) and patched corresponding project files (clrcompression.targets)
- Changed source code patcher to accommodate the new structure and fix some bugs.

Backwards compatibility was only tested with some 2020.x versions.

Patched mono dlls can be found at `builds/` folder.

Many thanks to this [blog post](https://blog.csdn.net/Ricardo0012/article/details/127103492) for saving hours of debugging.

### Tips for building and attaching

`toolset` version actually matters. Unity version 2020.x or bellow should use `v141`. Else dnSpy will mysterious fail to attach a game with patched mono.  VisualStudio 2017 Buildtools must be installed for `v141` to be available. 

Versions 2021.x seem to work fine with both `v141` and `v143` toolsets. In case of using `v143` a slight source code patch is needed, therefore, it's best to specify `--toolset v143` flag when running the umpatcher.

Rarely, a debugger connection might fail due to ports being reserved or used. Run command `netsh int ipv4 show excludedportrange tc` to list port ranges reserved by Windows. Make sure ports specified by `DNSPY_UNITY_DBG` and `DNSPY_UNITY_DBG2` environmental variables are not within these ranges.

Thanks to [funny-ppt](https://github.com/Funny-ppt/) for figuring this out.


---

This repo contains all files needed to build `mono.dll` & `mono-2.0-bdwgc.dll` with debugging support for Unity.

The `master` branch contains the original files. You have to check out the `dnSpy` branch to build everything. Use VS2017.

# Supporting a new Unity version for Dummies

- Pull in the latest Unity mono.dll source code (either `git pull` if you have it or `git clone https://github.com/Unity-Technologies/mono.git`)
- Get this repo and make sure `master` and `dnSpy` branches are at the latest commit (`git pull` in both branches)
- Compile `umpatcher` in this repo (you need VS2019 or later and .NET Core SDK 3.0 or later installed)
- Download the correct Unity editor version
	- https://unity3d.com/get-unity/download/archive
	- https://unity3d.com/unity/qa/lts-releases
	- https://unity3d.com/unity/qa/patch-releases
- Either install the Unity editor or extract the necessary .dlls with extractmono.bat
- If using extractmono.bat
    - 7zip must be installed and 7z.exe must be in PATH
    - extractmono.bat also assumes there are no - (hyphens) besides the one in the UnitySetup file name
    - Pass the directory the UnitySetupxxx.exe files(s) are in as the first argument
    - Pass the directory you want the file(s) to be extracted to as the second argument
    - Pass "mbe" or "both" as third parameter. "mbe" will extract mono-2.0-bdwgc.dll, "both" will extract both dlls, and no third parameter will extract mono.dll
    - Example: .\extractmono.bat C:\Users\Unfou\Downloads C:\Users\Unfou\Desktop\mono both
- Otherwise, if installing Unity editor:
- Locate the compiled `mono.dll` & `mono-2.0-bdwgc.dll` files, eg.:
	- `C:\Program Files\Unity\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win32_nondevelopment_mono\Data\Mono\EmbedRuntime\mono.dll`
	- `C:\Program Files\Unity\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\Data\Mono\EmbedRuntime\mono.dll`
	- `C:\Program Files\Unity\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll`
	- `C:\Program Files\Unity\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll`
- Get the timestamp stored in mono.dll's PE header
	- `umpatcher --timestamp "C:\path\to\the\correct\version\mono.dll"`
- Check out the correct version branch in the Unity mono repo, eg. if it's v5.4.3, the branch is called `unity-5.4`. Branches ending in `-mbe` use .NET 4.x assemblies.
	- Use `git branch -a` to see all remote branches
- `git checkout unity-5.4` (or whatever version you need)
- `git pull` (make sure it has the latest stuff)
- `gitk` to start a UI
	- Find the closest merge by comparing the merge date with the timestamp reported by `umpatcher` above
	- Remember the commit hash, you'll need it later
- Run umpatcher again to patch the code and commit it to the dnSpy-Unity-mono repo
	- `umpatcher 5.4.3 aa8a6e7afc2f4fe63921df4fe8a18cfd0a441d19 "C:\path\to\Unity-mono" "C:\path\to\dnSpy-Unity-mono"`

# Building `mono.dll` & `mono-2.0-bdwgc.dll`

- `dnSpy-Unity-mono-vZZZZ.x.sln` (Unity with .NET 2.0-3.5 assemblies), where `ZZZZ` is the major version number, eg. 4, 5, 2017, ...
	- Use configuration `Release_eglib`
	- Use platform `x86` or `x64`

- `dnSpy-Unity-mono-vZZZZ.x-V40.sln` (Unity with .NET 4.x assemblies), where `ZZZZ` is the major version number, eg. 2017, 2018, ...
	- Use configuration `Release`
	- Use platform `x86` or `x64`

# Commit hashes

A few versions have the same hash. `-mbe` = `MonoBleedingEdge` branch (.NET 4.x assemblies).

version | git hash
--------|---------

2019.4.23f1-mbe | 90cf2678d79ad248593837523bde01561ee6548e
2020.3.33f1-mbe | 734c22d2358f2a335d949022296f57d305ac24c1
2020.3.43f1-mbe | 98b3752f0139c20a72d9e70e68f8b0a679d6fd7b
2021.3.14f1-mbe | acb7cd69d120a28d0e0e2e3f4509de412fff2fb1
2021.3.18f1-mbe | 81a7696b7c1960113bebfe610ac3e693c7d41fce
