# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

ActionTimelineReplacement (ATR) is a Dalamud plugin for FFXIV. It rewrites four `ushort` fields on the in-memory `Action` data struct so that a chosen action plays a different start/end timeline, hit timeline, or cast VFX than the one shipped in `Action.exh`. The point is to let presets (or mods that ship presets) split actions that share a tmb so they can look different in-game. In-game command: `/atr`.

## Build

```
dotnet build                          # debug build, outputs to bin/<Configuration>/
dotnet build -c Release               # release build; DalamudPackager produces the .zip
dotnet restore --force-evaluate       # if packages.lock.json gets out of sync
```

There is no test project and no lint config — `dotnet build` is the only feedback loop.

`ActionTimelineReplacement.csproj` is platform-aware: on Windows it resolves Dalamud refs from `%APPDATA%\XIVLauncher\addon\Hooks\dev\`; on Linux from `/home/ted-jin/.xlcore/dalamud/Hooks/dev/` (hardcoded user path — change if developing as another user). `TargetFramework` is `net10.0-windows7.0`, `Platforms` is `x64`, `AllowUnsafeBlocks=true`. The Dalamud DLLs (`Dalamud`, `FFXIVClientStructs`, `Lumina*`, `Newtonsoft.Json`, `Dalamud.Bindings.ImGui/ImPlot`) are referenced with `<Private>false</Private>` — do not bundle them.

## Architecture

### Boot path
`Plugin.cs` → `pluginInterface.Create<Service>()` populates the `[PluginService]` statics on `Service` (the service locator). `MainModel` is loaded via `GetPluginConfig()` or constructed fresh, then `WindowManager` is instantiated last. `_disposables` is unwound in reverse on shutdown — order matters because `WindowManager` removes the `/atr` handler before `MainModel` flushes.

### Configuration tree (everything draws and persists itself)
```
MainModel  (IPluginConfiguration, [JsonConverter(MainModelJsonConverter)])
└── ActionTimelineReplacementSetModel   (= one preset: name, enabled, priority)
    └── ActionTimelineReplacementModel  (= one action being remapped)
        └── 4 × ActionOffsetModel       (AnimationStart, AnimationEnd, ActionTimelineHit, CastVfx)
```

Every leaf model derives from `BaseModel<T>` (`Models/BaseModel.cs`). The base class auto-calls `Service.Model.Save()` whenever `DrawImplementation()` reports a change, so subclasses never persist manually. `BaseModel<T>` also exposes `OnChanged` so non-UI listeners can react to user edits — `ActionOffsetAction` subscribes to its `_priority` and `_enable` chain through this event.

### The hot path: `Hookers/ActionOffsetAction.cs`
This is where the plugin actually mutates the game.

- `ActionOffsetDefinition` is a record of `(name, offset)` for the four ushort fields inside the per-action data struct: `CastVfx=10`, `ActionTimelineHit=12`, `AnimationEnd=32`, `AnimationStart=36`. **If you add a new field, add a new `ActionOffsetDefinition` static and pipe it through `ActionTimelineReplacementModel`'s constructor — do not invent a separate hook path.**
- The address of the per-action struct is obtained by sigscanning `GetActionData` (sig `E8 ?? ?? ?? ?? F6 40 3E 10`) and calling it with the action id. The returned pointer + offset is cached in `ValuePointer`; if the game ever returns a different pointer for the same id, the cache is replaced and a warning is logged (this is treated as anomalous).
- `ActionOffsetAction` is keyed by `(actionId, ActionOffsetDefinition)` in a static `Dictionary<uint, Dictionary<ActionOffsetDefinition, ActionOffsetAction>>` — there is exactly one writer per (action, field). Every `ActionOffsetModel` registers itself with its action; the action picks the model with `MaxBy(Priority)` among models where `Enable` is true (Enable = AND of `EnableReplacement`, set-level `Enabled`, and replacement-level `Enabled` — all three must be on). When the active model changes, the new value (or `DefaultValue` if no model qualifies) is written back through the pointer. `DefaultValue` is captured lazily on first read and is what the "Reply" reset button restores.
- Writes are unsafe pointer writes — `*ValuePointer = value`. There is no Dalamud hook (`IGameInteropProvider` is in `Service` but unused). This means changes apply immediately and persist for the session; on plugin shutdown the active models all unregister and the field falls back to `DefaultValue`.

### Persistence and import/export — two different formats
- **Plugin config** (`Service.PluginInterface.SavePluginConfig`): handled by `Serialization/MainModelJsonConverter.cs`. Top-level keys: `Version`, `EnableReplacement`, `AdvancedMode`, `ActionTimelineReplacements[]`. Each set has `Name`, `Enabled`, `Priority`, and `Replacements` as a `{ actionId: { Replacement: {...}, Enabled: bool } }` map.
- **Preset import/export** (the user-facing JSON files): defined inline in `ActionTimelineReplacementSetModel.Import`/`Export`. Format is a flat `{ actionId: { AnimationStart, AnimationEnd, ActionTimelineHit, CastVfx } }` dictionary — no name, no enabled, no priority. Imported sets default to `enabled=true, priority=0` and inherit the file basename as `Name`. **These are not interchangeable formats — do not unify them without thinking about preset files modders have already shipped.**

### UI
`Windows/ConfigWindow.cs` is a thin shell; everything is drawn by the model tree itself via `IDrawItem.Draw()`. `WindowManager` registers `/atr` and the Dalamud OpenConfigUi/OpenMainUi hooks. `Helpers/ActionLookup.cs` lazy-loads the Lumina `Action` sheet for the search popup (`ScoreString` is a Levenshtein-with-substring fast-path) and provides `GetOriginal(id)` which is what the "+" search uses to seed a new replacement with the action's stock values.

## Conventions worth knowing
- `Service` is the only static singleton — reach for it instead of plumbing `IDalamudPluginInterface` through constructors.
- `BaseModel.Draw()` saves on every change. Don't add a separate save call in your subclass.
- A model's `GetHashCode()` is used as the ImGui id (`ImRaii.PushId(GetHashCode())`) — don't override `Equals`/`GetHashCode` on model types.
- `MainModel.Version` exists in the schema but is currently always `0`; use it if you ship a breaking config change.
