# Repository Guidelines

## Project Structure & Module Organization

This is a Unity 6 visual-novel project (editor version `6000.0.62f1`). Keep project code under `Assets/Scripts/`; visual-novel effects and their custom editor tooling live in `Assets/Scripts/VNEffects/` and `Assets/Scripts/VNEffects/Editor/`. Scenes are in `Assets/Scenes/`, with `SampleScene.unity` currently included in Build Profiles. Store reusable objects in `Assets/Prefabs/`, shaders in `Assets/Shaders/`, render-pipeline settings in `Assets/Settings/`, and runtime-loaded content in `Assets/Resources/`.

Treat `Assets/Plugins/`, `Assets/TextMesh Pro/`, and `Assets/Editor Default Resources/Dialogue System/` as vendor-managed code. Do not edit generated folders (`Library/`, `Temp/`, `Logs/`, `obj/`) or generated `.csproj`/`.sln` files. Commit every Unity asset together with its `.meta` file.

## Build, Test, and Development Commands

- Open the repository with Unity Hub using Unity `6000.0.62f1` for normal development.
- In Unity, use **File > Build Profiles** to build the enabled scene.
- Run Edit Mode tests headlessly:
  `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -quit`
- Run Play Mode tests by replacing `EditMode` with `PlayMode`.

Use the full Unity executable path if `Unity.exe` is not on `PATH`. The repository has no custom build script; do not rely on generated solution files as the authoritative build configuration.

## Coding Style & Naming Conventions

Follow the existing C# style: four-space indentation, braces on new lines, `PascalCase` for types and public members, `camelCase` for parameters and public serialized fields, and `_camelCase` for private fields. Keep MonoBehaviour filenames identical to their primary class (for example, `VNWeatherController.cs`). Use the established `VN` prefix for visual-novel effect components. Place editor-only code in an `Editor/` directory.

## Testing Guidelines

Unity Test Framework `1.6.0` is installed, but no project-owned test assemblies currently exist. Add tests under `Assets/Tests/EditMode/` or `Assets/Tests/PlayMode/`, with an appropriate `.asmdef`. Name fixtures `FeatureTests` and test methods after behavior, such as `SetWeather_StopsPreviousSystem`. Before submitting, run relevant tests and manually verify scene/prefab changes in Play Mode.

## Commit & Pull Request Guidelines

No Git history is available in this checkout. Use short, imperative commit subjects, optionally scoped, such as `effects: add rain transition`. Keep commits focused and include related `.meta` files. Pull requests should describe player-visible behavior, list test steps, link related issues, and include screenshots or short recordings for scene, UI, shader, or animation changes. Call out package or `ProjectSettings/` changes explicitly.
