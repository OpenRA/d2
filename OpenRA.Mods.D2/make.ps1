if ($args.Length -eq 0)
{
	$command = "default"
}
elseif ($args.Length -eq 1)
{
    $command = $args

    if ($command -eq "help"   -or
	$command -eq "h"      -or
	$command -eq "--help" -or
	$command -eq "-h"     -or
	$command -eq "/h"     -or
	$command -eq "/?")
    {
	echo "Target list:"
	echo ""
	echo "  default   Builds the mod dll."
	echo "  clean     Removes the mod dll."
	echo "  deps      Copies the mod's dependencies into the"
	echo "              'OpenRA.Mods.D2/dependencies' directory."
	echo ""
	echo "  'dependencies' is an alias for 'deps'"
	echo ""
	exit
    }
    elseif ($command -eq "dependencies")
	$command = "deps"
}

cake -target="$command"
