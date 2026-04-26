PROJECT := Thingy52/Thingy52.csproj
BLE_ABSTRACTIONS_PROJECT := Thingy52.Ble.Abstractions/Thingy52.Ble.Abstractions.csproj
CONFIGURATION ?= Debug
TFM := net10.0-maccatalyst
RUN_RID ?= $(shell [ "$$(uname -m)" = "arm64" ] && echo maccatalyst-arm64 || echo maccatalyst-x64)
APP_BUNDLE := Thingy52/bin/$(CONFIGURATION)/$(TFM)/$(RUN_RID)/Thingy52.app
NUGET_OUT := artifacts/nuget

.PHONY: restore build run clean pack-ble-abstractions

restore:
	dotnet restore $(PROJECT)

build:
	dotnet build $(PROJECT) -c $(CONFIGURATION) -f $(TFM)

run: build
	@test -d "$(APP_BUNDLE)" || (echo "App-Bundle nicht gefunden: $(APP_BUNDLE)" && exit 1)
	open "$(APP_BUNDLE)"

clean:
	dotnet clean $(PROJECT) -c $(CONFIGURATION) -f $(TFM)

pack-ble-abstractions:
	mkdir -p $(NUGET_OUT)
	dotnet pack $(BLE_ABSTRACTIONS_PROJECT) -c Release -o $(NUGET_OUT)