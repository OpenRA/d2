############################# INSTRUCTIONS #############################
#
# to compile, run:
#   make [DEBUG=false]
#
# to compile with development tools, run:
#   make all [DEBUG=false]
#
# to check the official mods for erroneous yaml files, run:
#   make test
#
# to check the official mod dlls for StyleCop violations, run:
#   make check
#
# to generate documentation aimed at modders, run:
#   make docs
#
# to install, run:
#   make [prefix=/foo] [bindir=/bar/bin] install
#
# to install with development tools, run:
#   make [prefix=/foo] [bindir=/bar/bin] install-all
#
# to install Linux startup scripts, desktop files and icons:
#   make install-linux-shortcuts [DEBUG=false]
#
# to uninstall, run:
#   make uninstall
#
# for help, run:
#   make help
#
# to start the game, run:
#   openra



############################## TOOLCHAIN ###############################
#
SDK         ?=
CSC         = mcs $(SDK)
CSFLAGS     = -nologo -warn:4 -codepage:utf8 -unsafe -warnaserror
DEFINE      = TRACE
COMMON_LIBS = System.dll System.Core.dll System.Data.dll System.Data.DataSetExtensions.dll System.Drawing.dll System.Xml.dll dependencies/Eluant.dll

DEBUG = true
ifeq ($(DEBUG), $(filter $(DEBUG),false no n off 0))
CSFLAGS   += -debug:pdbonly -optimize+
else
CSFLAGS   += -debug:full -optimize-
DEFINE    := DEBUG;$(DEFINE)
endif



######################### UTILITIES/SETTINGS ###########################
#
# install locations
prefix ?= /usr/local
datarootdir ?= $(prefix)/share
datadir ?= $(datarootdir)
mandir ?= $(datarootdir)/man/
bindir ?= $(prefix)/bin
libdir ?= $(prefix)/lib
gameinstalldir ?= $(libdir)/openra

BIN_INSTALL_DIR = $(DESTDIR)$(bindir)
DATA_INSTALL_DIR = $(DESTDIR)$(gameinstalldir)

# install tools
RM = rm
RM_R = $(RM) -r
RM_F = $(RM) -f
RM_RF = $(RM) -rf
CP = cp
CP_R = $(CP) -r
INSTALL = install
INSTALL_DIR = $(INSTALL) -d
INSTALL_PROGRAM = $(INSTALL) -m755
INSTALL_DATA = $(INSTALL) -m644

# program targets
VERSION     = $(shell git name-rev --name-only --tags --no-undefined HEAD 2>/dev/null || echo git-`git rev-parse --short HEAD`)

# dependencies
UNAME_S := $(shell uname -s)
ifeq ($(UNAME_S),Darwin)
os-dependencies = osx-dependencies
else
os-dependencies = linux-dependencies
endif

BASE_DIR = ../../..

######################## PROGRAM TARGET RULES ##########################
#
# Core binaries

game_TARGET = dependencies/OpenRA.Game.exe
game_KIND = winexe

# Platform dlls
#pdefault_TARGET = OpenRA.Platforms.Default.dll
#pdefault_KIND = library

# Mods Common
mod_common_TARGET = dependencies/OpenRA.Mods.Common.dll
mod_common_KIND = library

##### Official Mods #####

STD_MOD_LIBS	= $(game_TARGET)
STD_MOD_DEPS	= $(STD_MOD_LIBS)

# Red Alert
mod_ra_TARGET = dependencies//OpenRA.Mods.RA.dll
mod_ra_KIND = library

# Command and Conquer
mod_cnc_TARGET = dependencies//OpenRA.Mods.Cnc.dll
mod_cnc_KIND = library

# Dune 2000
mod_d2k_TARGET = dependencies//OpenRA.Mods.D2k.dll
mod_d2k_KIND = library

# Dune 2
mod_d2_SRCS := $(shell find . -iname '*.cs')
mod_d2_TARGET = ../OpenRA.Mods.D2.dll
mod_d2_KIND = library
mod_d2_DEPS = 
mod_d2_LIBS = $(COMMON_LIBS) $(STD_MOD_LIBS) $(mod_common_TARGET)
PROGRAMS += mod_d2
mod_d2: $(mod_d2_TARGET)

# Tiberian Sun
#mod_ts_TARGET = mods/ts/OpenRA.Mods.TS.dll
#mod_ts_KIND = library

check-scripts:
	@echo
	@echo "Checking for Lua syntax errors..."
	@luac -p $(shell find ../maps/* -iname '*.lua')
	@luac -p $(shell find $(BASE_DIR)/lua/* -iname '*.lua')

check: mods
	@echo
	@echo "Checking for code style violations in OpenRA.Mods.D2..."
	@mono --debug $(BASE_DIR)/OpenRA.Utility.exe ra --check-code-style .

test: mods
	@echo
	@echo "Testing D2 mod MiniYAML..."
	@mono --debug $(BASE_DIR)/OpenRA.Utility.exe d2 --check-yaml


##### Launchers / Utilities #####

# Patches binary headers to work around a mono bug
fixheader.exe: $(BASE_DIR)/packaging/fixheader.cs
	@echo CSC fixheader.exe
	@$(CSC) $(BASE_DIR)/packaging/fixheader.cs $(CSFLAGS) -out:fixheader.exe -t:exe $(COMMON_LIBS:%=-r:%)

# Generate build rules for each target defined above in PROGRAMS
define BUILD_ASSEMBLY

$$($(1)_TARGET): $$($(1)_SRCS) Makefile $$($(1)_DEPS) fixheader.exe
	@echo CSC $$(@)
	@$(CSC) $$($(1)_LIBS:%=-r:%) \
		-out:$$(@) $(CSFLAGS) $$($(1)_FLAGS) \
		-define:"$(DEFINE)" \
		-t:"$$($(1)_KIND)" \
		$$($(1)_EXTRA) \
		$$($(1)_SRCS)
	@mono fixheader.exe $$(@) > /dev/null
	@test `echo $$(@) | sed 's/^.*\.//'` = "dll" && chmod a-x $$(@) || ``
	@$$($(1)_EXTRA_CMDS)
endef

$(foreach prog,$(PROGRAMS),$(eval $(call BUILD_ASSEMBLY,$(prog))))



########################## MAKE/INSTALL RULES ##########################
#
default: mods

mods: mod_d2
	@-$(RM_F) fixheader.exe
	@-$(RM_F) fixheader.exe.mdb

all: dependencies mods

clean:
	@-$(RM_F) *.exe *.dylib *.dll.config ../*.dll ../*.mdb *.mdb *.dll *.resources
	@-$(RM_RF) ./*/bin ./*/obj

distclean: clean

cli-dependencies:
#	@./thirdparty/fetch-thirdparty-deps.sh
#	@ $(CP_R) thirdparty/download/*.dll .
#	@ $(CP_R) thirdparty/download/*.dll.config .

linux-dependencies: cli-dependencies linux-native-dependencies

linux-native-dependencies:
#	@./thirdparty/configure-native-deps.sh

windows-dependencies:
#	@./thirdparty/fetch-thirdparty-deps-windows.sh

osx-dependencies: cli-dependencies
#	@./thirdparty/fetch-thirdparty-deps-osx.sh
#	@ $(CP_R) thirdparty/download/osx/*.dylib .
#	@ $(CP_R) thirdparty/download/osx/*.dll.config .

dependencies: $(os-dependencies)
#	@./thirdparty/fetch-geoip-db.sh
#	@ $(CP) thirdparty/download/GeoLite2-Country.mmdb.gz .

all-dependencies: cli-dependencies windows-dependencies osx-dependencies

version: ../mod.yaml
	@for i in $? ; do \
		awk '{sub("Version:.*$$","Version: $(VERSION)"); print $0}' $${i} > $${i}.tmp && \
		awk '{sub("\tmodchooser:.*$$","\tmodchooser: $(VERSION)"); print $0}' $${i}.tmp > $${i}.tmp2 && \
		awk '{sub("/[^/]*: User$$", "/$(VERSION): User"); print $0}' $${i}.tmp2 > $${i} && \
		rm $${i}.tmp $${i}.tmp2; \
	done

docs: mods version
	@mono --debug $(BASE_DIR)/OpenRA.Utility.exe all --docs > DOCUMENTATION.md
	@mono --debug $(BASE_DIR)/OpenRA.Utility.exe all --lua-docs > Lua-API.md

#install: install-core
#
#install-all: install-core install-tools
#
#install-linux-shortcuts: install-linux-scripts install-linux-icons install-linux-desktop
#
#install-core: default
#	@$(CP_R) .. "$(DATA_INSTALL_DIR)/mods/d2"
#	@$(INSTALL_PROGRAM) $(mod_d2_TARGET) "$(DATA_INSTALL_DIR)/mods/d2"
#
#ifneq ($(UNAME_S),Darwin)
#	@$(CP) *.sh "$(DATA_INSTALL_DIR)"
#endif
#
#uninstall:
#	@-$(RM_R) "$(DATA_INSTALL_DIR)/mods/d2"

help:
	@echo 'to compile, run:'
	@echo '  make [DEBUG=false]'
	@echo
	@echo 'to compile with development tools, run:'
	@echo '  make all [DEBUG=false]'
	@echo
	@echo 'to check the official mods for erroneous yaml files, run:'
	@echo '  make test'
	@echo
	@echo 'to generate documentation aimed at modders, run:'
	@echo '  make docs'
	@echo
	@echo 'to install, run:'
	@echo '  make [prefix=/foo] [bindir=/bar/bin] install'
	@echo
	@echo 'to install with development tools, run:'
	@echo '  make [prefix=/foo] [bindir=/bar/bin] install-all'
	@echo
	@echo 'to install Linux startup scripts, desktop files and icons'
	@echo '  make install-linux-shortcuts [DEBUG=false]'
	@echo
	@echo 'to uninstall, run:'
	@echo '  make uninstall'
	@echo
	@echo 'to start the game, run:'
	@echo '  openra'




########################### MAKEFILE SETTINGS ##########################
#
.DEFAULT_GOAL := default

.SUFFIXES:

.PHONY: core tools package all mods clean distclean dependencies version $(PROGRAMS) nunit
