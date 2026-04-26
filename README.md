# csharp-maui-thingy52

Active MAUI BLE application for ongoing development.

## Status

- Active project for feature development and maintenance
- Target frameworks: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`
- BLE abstraction package available in `Thingy52/Thingy52.Ble.Abstractions`

## Build

```bash
cd Thingy52/Thingy52
dotnet build -f net10.0-android -v minimal
```

## Optional Packaging

Build NuGet package for BLE abstractions:

```bash
cd Thingy52
make pack-ble-abstractions
```