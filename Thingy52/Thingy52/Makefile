MAKEFILE_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
ROOT_DIR := $(abspath $(MAKEFILE_DIR)/..)

.PHONY: restore build run clean clean-artifacts rebuild run-rebuild pack-ble-abstractions run-maccatalyst-shiny run-maccatalyst-pluginble build-maccatalyst-shiny build-maccatalyst-pluginble rebuild-maccatalyst-shiny rebuild-maccatalyst-pluginble

restore build run clean clean-artifacts rebuild run-rebuild pack-ble-abstractions run-maccatalyst-shiny run-maccatalyst-pluginble build-maccatalyst-shiny build-maccatalyst-pluginble rebuild-maccatalyst-shiny rebuild-maccatalyst-pluginble:
	$(MAKE) -C $(ROOT_DIR) $@
