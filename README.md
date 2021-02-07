# hardlinq

*This command-line utility compares the contents of dirA and dirB, then creates hard links for any missing files in dirB.*<br />
*Its purpose is to duplicate (or loosely sync) two directories without duplicating hard disk usage.*<br />

	Usage: hardlinq <sourceDir> <destDir> [--test] [--showcommon] [--findlinks]
	  --test	test mode (don't write, show diff files only)
	  --strip	strip source path from test output
	  --showcommon	show common files between the two directories
	  --comparelength	in addition to name, also compare files by length in bytes
	  --findlinks	find all links in destDir (requires Sysinternals findlinks.exe in PATH)
	  --longpaths	set registry value LongPathsEnabled=1 (requires admin)

	Notes:
	  Program is alpha. Not recommended for use with critical data.
	  Both sourceDir and destDir must be provided, and they must exist.
	  Use full paths, with quotes if they contain spaces.
	  Long paths may fail if you haven't opted-in by registry.
