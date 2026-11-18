# Hell Frontiers

## One-Mechanic Action Platformer

Hell Frontiers is a single-player pixel-art action-platformer for Android. You play an angel who fell into Hell, and you must defeat emotion-themed bosses to recover your shattered power and return to Heaven.

- **Engine:** Unity 6.6
- **Platform:** Android (IL2CPP, ARM64)
- **Genre:** Platform / Action, 1 player
- **Author:** Javier Cáceres
- **Module:** Mobile Game Development, CA1 (Platform Readiness and Publication Awareness)

## Test device

| Item | Value |
|---|---|
| Phone model | Xiaomi Poco M4 Pro |
| Android version | 13 (TP1A.220624) |

## Cheat menu (for testing)

The game has a cheat menu. It is a button that opens a menu with shortcuts to all the important places in the game. I use it to look for bugs faster, and it lets the lecturer check that everything works without playing through the whole game. It is a testing tool, not part of normal play.

- **Where the button is:** [describe where it is on the screen and in which scenes]
- **Places it can take you to:** [list them, for example the tutorial, each boss room, the floor-is-lava climb]

## Requirements

- Unity 6.6 installed through Unity Hub, with the **Android Build Support** module (including OpenJDK and Android SDK & NDK Tools)
- An Android phone with **Developer options** and **USB debugging** enabled
- [Android platform-tools](https://developer.android.com/tools/releases/platform-tools) (`adb`) available on your PATH
- A release keystore (see [Signing](#signing))

## Build steps (Release APK)

1. Open the project in Unity 6.6.
2. Open **File > Build Profiles**, select **Android** and click **Switch Platform**.
3. In the Android build profile, make sure **Build App Bundle (Google Play)** is **off** and **Development Build** is **off**. CA1 needs an APK for sideloading.
4. Open **Player Settings > Other Settings** and set:
   - **Scripting Backend:** IL2CPP (set this first, otherwise ARM64 stays greyed out)
   - **Target Architectures:** ARM64 (ARMv7 optional)
   - **Target API Level:** Automatic (highest installed)
   - **Package Name:** `com.CaceresJavier.Hellfrontiers`
   - **Version:** `1.0` (see [Versioning](#versioning))
   - **Bundle Version Code:** `1` for the first build; increment on every new build
5. Open **Player Settings > Publishing Settings**, tick **Custom Keystore**, select the release keystore, choose the alias and enter the passwords. Do the same for the key if it has its own password.
6. Click **Build** and save the APK as `releases/HellFrontiers-<version>-release-arm64.apk`.

## Install on a device (ADB)

```
adb devices
adb install -r releases/HellFrontiers-1.0-release-arm64.apk
```

`-r` reinstalls the app and keeps its data. The output of this command is stored in `docs/CA1/install-proof.txt`, and an on-device screenshot is stored in `docs/CA1/device-screenshot.png`.

If Android shows "App not installed" when updating, the `versionCode` was not increased or the APK was signed with a different keystore. Rebuild with the same keystore and a higher `versionCode`.

## Signing

The release APK is signed with a keystore created in Unity's **Keystore Manager**. The keystore itself and its passwords are **not** stored in this repository.

| Item | Value |
|---|---|
| Keystore path | `hellfrontiers.keystore` |
| Key alias | `hellfrontiers` |
| Validity | `<creation date>` to `<expiry date>` *(fill in; 25 years or more is recommended)* |

To read these values from the keystore:

```
keytool -list -v -keystore <path-to-keystore>
```

Notes:

- Passwords are stored in a password manager, never in the repo or the submission ZIP.
- `*.keystore` and `*.jks` are listed in `.gitignore`.
- The same keystore must be used for every future update, otherwise installed copies cannot be updated.
- A backup of the keystore is kept outside the project folder.

## Versioning

The package name never changes between builds. The `versionCode` goes up on every build, because Android refuses to install a lower `versionCode` over a higher one (unless `adb install -d` is used).

| Build | Version name | versionCode | Notes |
|---|---|---|---|
| 1 | 1.0 | 1 | First signed release APK (splash screen and draft main menu) |

## CA2: Playable build (horizontal game)

Hell Frontiers is built as a horizontal game, not as a vertical slice: all of its content (the map, the bosses, the enemies and the orbs) is in the build, and the whole game can be played. The CA2 build is a release-signed APK that installs without ADB, version name `2.0`, tagged `v0.2-CA2` in the repository.

### Device target

| Item | Value |
|---|---|
| Phone | Xiaomi Poco M4 Pro, Android 13 |
| SoC and RAM | MediaTek Helio G96, 6 GB |
| Screen | 90 Hz, landscape |
| Architecture | ARM64 (IL2CPP) |
| Frame rate | 60 fps in Balanced mode, 90 fps in High FPS mode |

### Controls

All controls are on-screen touch buttons.

| Action | Control |
|---|---|
| Move left and right | On-screen button |
| Jump | On-screen button |
| Basic attack (four directions, also in the air) | On-screen button |
| Feather (ranged attack, needs the Ranged attack orb) | On-screen button |
| Dash (needs the Dash orb) | On-screen button |
| Change active orb | On-screen button |
| Interact (activate a checkpoint) | On-screen button |
| Open the cheat menu | On-screen button |

The ice shield has no button: it blocks the next enemy attack by itself and recharges after a while (it needs the Shield orb).

### Performance evidence

All numbers come from the Unity Profiler and `adb` on the test phone, with build v2.0 (015d8ae7).

| Metric | Result |
|---|---|
| Frame time (tutorial boss and Zone 1, 60 s) | 16.68 ms mean, 17.10 ms 99th percentile |
| GPU time / CPU time | 14.121 ms / 8.156 ms |
| SetPass calls | 10 |
| GC Alloc | 9 B per frame |
| Cold start / warm start (`TotalTime`) | 312 ms median / 91 ms |
| APK size | 32.9 MB |
| Memory | PSS 342.6 MB, textures 79 MB, managed heap 7.2 MB |

## AI usage

I used AI tools during this project, mainly Claude. I am responsible for everything I submit and I can explain it.

- **Documents.** I used Claude to help me create the documents (the performance baseline, the reflective journal, the known issues and this README). I chose it because it is better than me at explaining and writing, and it helped me find out how to get the information. The measurements were taken by me on my phone and the decisions are mine.
- **Bosses.** The AI helped me create the fear boss and the final boss, starting from the bosses I had already made. For the first boss (AngerBoss) it helped with the triggers and the boss room.
- **Checkpoint and floor is lava.** The AI helped me with these two parts and corrected my language (C# syntax and conventions).
- **Art.** I made all the original art. The AI helped me create the different frames for the animations.
- **Code base.** My base scripts and state machine come from my own earlier university projects, and the AI helped me adapt them to Unity. I wrote `MeleeEnemy` and `BaseOrb` myself and the AI created the other enemies and orbs from them. It also helped me understand compilation and Unity errors, and to move from C++ to C#.
- **What I did myself.** The game design, the original art, the Tiled map, the first version of the player, enemy, boss and orb systems, and all the testing on the device.

## Repository layout

```
releases/    Release APKs (HellFrontiers-<version>-release-arm64.apk)
docs/        Design and submission documents
  CA1/       install-proof.txt, device-screenshot.png, store-assets-checklist.md,
             descriptions.md, privacy-statement.md, mda-onepager.md
  dev-journal.md
Assets/      Unity project assets
README.md    This file
```

## Branching

Currently the repository has a single branch, `main`, which holds the stable, working state. New work will use semantic branch names:

- `feature/<name>`: new features, for example `feature/main-menu`
- `fix/<name>`: bug fixes, for example `fix/splash-scaling`
- `release/<version>`: release preparation, for example `release/1.1`

## Documentation

- [MDA one-pager](docs/CA1/mda-onepager.md)
- [Store assets checklist](docs/CA1/store-assets-checklist.md)
- [Store descriptions](docs/CA1/descriptions.md)
- [Data-use and privacy statement](docs/CA1/privacy-statement.md)
- [Development journal](docs/dev-journal.md)