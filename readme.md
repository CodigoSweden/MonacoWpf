# Monaco Wpf
A reusable wpf control that embedds the monaco editor. 
- MonacoEditor 
- MonacoDiffEditor
- LanguageService for a C# based DSL

## Usage
- Set Browser emulation
- Bind To Language And Value
- csharp, template...

## Build
From command line
- Installera Node.js
- npm install gulp-cli -g
- npm install

## Design
- website
- Embedded resource with a website
- Unpacked to appdata
- Simplehttpserver
- webbrowser control

## TODO
- signatur och highlithning
- json schema från csharp typ m.m
- add typescript definitions
- Func to filter intelisense list
- Kan man få till någon bra template sak..
- Kompilera skripten och returnera func m.m
- Async skripts


## Buggar
- Fel rad ibland
- Remove handler i destructor och då man byter context..
- Ifall man väljer ett skript, går till en annan tab och tillbaka så syns inte skriptet längre…

