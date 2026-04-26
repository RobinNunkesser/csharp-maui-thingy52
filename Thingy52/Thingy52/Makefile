MAKEFILE_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
ROOT_DIR := $(abspath $(MAKEFILE_DIR)/..)

.PHONY: restore build run clean pack-ble-abstractions

restore build run clean pack-ble-abstractions:
	$(MAKE) -C $(ROOT_DIR) $@
