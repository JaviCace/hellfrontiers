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
| Keystore path | `user.keystore` |
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