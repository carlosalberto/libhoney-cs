.PHONY: test clean clean-build build

LIBHONEY_VERSION=0.9.1
LIBHONEY_DOCUMENTATION_DIR=/tmp/LibHoneyHtmlDocumentation

NUGET_KEY=
NUGET_SOURCE=https://www.nuget.org/api/v2/package

XUNIT_VERSION=2.1.0

build:
	xbuild Src/LibHoney.sln

build-release:
	xbuild Src/LibHoney.sln /p:Configuration=Release

clean: clean-build

clean-build:
	xbuild /target:clean Src/LibHoney.sln

test: build
	mono Src/packages/xunit.runner.console.$(XUNIT_VERSION)/tools/xunit.console.exe Src/LibHoney.Tests/bin/Debug/LibHoney.Tests.dll -parallel none

test-release: build-release
	mono Src/packages/xunit.runner.console.$(XUNIT_VERSION)/tools/xunit.console.exe Src/LibHoney.Tests/bin/Release/LibHoney.Tests.dll -parallel none

docs: build-release
	monodocer -pretty -importslashdoc:Src/LibHoney/bin/Release/LibHoney.xml -assembly:Src/LibHoney/bin/Release/LibHoney.dll -path:/tmp/LibHoneyDocs
	mdoc export-html -o $(LIBHONEY_DOCUMENTATION_DIR) /tmp/LibHoneyDocs/

publish: test-release
	(cd Src/LibHoney && nuget pack -properties Configuration=Release)
	(cd Src/LibHoney && nuget push LibHoney.$(LIBHONEY_VERSION).nupkg -Source $(NUGET_SOURCE) -ApiKey $(NUGET_KEY))
	(cd Src/LibHoney && rm LibHoney.$(LIBHONEY_VERSION).nupkg)
