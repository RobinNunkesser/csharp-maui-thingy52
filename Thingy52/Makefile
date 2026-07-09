PROJECT := Thingy52/Thingy52.csproj
BLE_ABSTRACTIONS_PROJECT := Thingy52.Ble.Abstractions/Thingy52.Ble.Abstractions.csproj
CONFIGURATION ?= Debug
TFM := net10.0-maccatalyst
RUN_RID ?= $(shell [ "$$(uname -m)" = "arm64" ] && echo maccatalyst-arm64 || echo maccatalyst-x64)
APP_BUNDLE := Thingy52/bin/$(CONFIGURATION)/$(TFM)/$(RUN_RID)/Thingy52.app
NUGET_OUT := artifacts/nuget
THINGY_BLE_BACKEND ?= Shiny

.PHONY: restore build run clean clean-artifacts rebuild run-rebuild pack-ble-abstractions run-maccatalyst-shiny run-maccatalyst-pluginble build-maccatalyst-shiny build-maccatalyst-pluginble rebuild-maccatalyst-shiny rebuild-maccatalyst-pluginble

restore:
	dotnet restore $(PROJECT)

build:
	dotnet build $(PROJECT) -c $(CONFIGURATION) -f $(TFM) -p:RuntimeIdentifier=$(RUN_RID) -p:SkipCodesign=true -p:ThingyBleBackend=$(THINGY_BLE_BACKEND)

run: build
	@test -d "$(APP_BUNDLE)" || (echo "App-Bundle nicht gefunden: $(APP_BUNDLE)" && exit 1)
	open "$(APP_BUNDLE)"

clean-artifacts:
	rm -rf Thingy52/bin Thingy52/obj || true

rebuild: clean clean-artifacts build

run-rebuild: rebuild
	@test -d "$(APP_BUNDLE)" || (echo "App-Bundle nicht gefunden: $(APP_BUNDLE)" && exit 1)
	open "$(APP_BUNDLE)"

build-maccatalyst-shiny:
	$(MAKE) build THINGY_BLE_BACKEND=Shiny

run-maccatalyst-shiny:
	$(MAKE) run-rebuild THINGY_BLE_BACKEND=Shiny

build-maccatalyst-pluginble:
	$(MAKE) build THINGY_BLE_BACKEND=PluginBle

run-maccatalyst-pluginble:
	$(MAKE) run-rebuild THINGY_BLE_BACKEND=PluginBle

rebuild-maccatalyst-shiny:
	$(MAKE) rebuild THINGY_BLE_BACKEND=Shiny

rebuild-maccatalyst-pluginble:
	$(MAKE) rebuild THINGY_BLE_BACKEND=PluginBle

clean:
	dotnet clean $(PROJECT) -c $(CONFIGURATION) -f $(TFM)

pack-ble-abstractions:
	mkdir -p $(NUGET_OUT)
	dotnet pack $(BLE_ABSTRACTIONS_PROJECT) -c Release -o $(NUGET_OUT)