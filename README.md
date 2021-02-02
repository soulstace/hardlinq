# hardlinq

*This command-line utility compares the contents of dirA and dirB, then creates hard links for any missing files in dirB.*<br />
*Its purpose is to duplicate (or loosely sync) two directories without duplicating hard disk usage.*<br />

	Usage: hardlinq <sourceDir> <destDir> [-t] [--findlinks]
	  -t	test mode (don't write, show diff files only)
	  --findlinks	find all links in sourceDir (requires Sysinternals findlinks.exe in PATH)

	Notes:
	  Program is alpha. Not recommended for use with critical data.
	  Use full paths, with quotes if they contain spaces.
	  Paths longer than 255 characters are not yet handled and will likely fail.
	  Both <sourceDir> and <destDir> directories must exist.
