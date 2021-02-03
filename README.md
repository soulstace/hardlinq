# hardlinq

*This command-line utility compares the contents of dirA and dirB, then creates hard links for any missing files in dirB.*<br />
*Its purpose is to duplicate (or loosely sync) two directories without duplicating hard disk usage.*<br />

	Usage: hardlinq <sourceDir> <destDir> [-t] [--strip] [--findlinks] 
	  -t	test mode (don't write, show diff files only)
	  --strip	strip source path from test output (combine with -t)
	  --findlinks	find all links in destDir (requires Sysinternals findlinks.exe in PATH)

	Notes:
	  Program is alpha. Not recommended for use with critical data.
	  Use full paths, with quotes if they contain spaces.
	  Paths longer than 255 characters are not yet handled and will likely fail.
	  Both <sourceDir> and <destDir> directories must exist.
