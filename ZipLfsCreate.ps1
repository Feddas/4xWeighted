# Compress LFS based files into a zip
# To use
#  1. place this script in the root folder
#  2. modify the contents of $lfsAssetFiles to point to files relative to this root folder
#  3. modify $zipDestination to be where you want the resultant zip to be placed
# based off of https://stackoverflow.com/a/51394271

# this should match files being .gitignored.
# comma is needed by powershell to look at the next line. i.e. don't start a line with a comma ", \Packages"
$lfsAssetFiles = 
#"\Test\content.txt",
#"\Test2",
"\Assets\Art\AudioLfs",
"\Assets\Art\IconLfs",
"\Assets\Art\MeshLfs",
"\RawArt"

# a package.json file should be used for versioning
$packagePath = Join-Path $PSScriptRoot "\Assets\package.json"
$zipDestination = "4xWeighted" # This starts as a fallback filename. can be an empty string if package.json is valid.

# ## ### End of customizable fields ### ## #

# Contents of the zip file will be structured outside of a zip file. Because placing files into specific folders of an existing zip is too difficult.
$zipStruct = $PSScriptRoot + "\zipStruct"

# Where the zip file will be created
If(Test-path $packagePath)
{
  $package = Get-Content -Raw -Path $packagePath | ConvertFrom-Json
  $zipDestination = $package.displayName + $package.version
} # else fallback value is used
$zipDestination = "\Builds\Lfs" + $zipDestination + ".zip"
$zipDestination = Join-Path $PSScriptRoot $zipDestination

# remove files from previous runs of this script
If(Test-path $zipStruct) {Remove-item $zipStruct -Recurse}
If(Test-path $zipDestination) {Remove-item $zipDestination}

Foreach ($entry in $lfsAssetFiles)
{
  # form absolute path to source each file to be included in the zip
  $sourcePath = $PSScriptRoot + $entry;
  
  # get the parent directories of the path. If the entry itself is a directory, we still only need the parent as the directory will be created when it is copied over.
  # NO if ((Get-Item $sourcePath) -is [System.IO.DirectoryInfo])
  $entryPath = Split-Path -Parent $entry

  # form what the path will look like in the destination
  $entryPath = $zipStruct + $entryPath

  # ensure the folders to the entry path exist
  $createdPath = New-Item -Force -ItemType Directory $entryPath

  # copy the file or directory
  Copy-Item -Recurse -Force $sourcePath $createdPath
}

# create a zip file https://blogs.technet.microsoft.com/heyscriptingguy/2015/page/59/
Add-Type -AssemblyName "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($zipStruct, $zipDestination)
# Compress-Archive doesn't work here because it includes the "zipStruct" folder: Compress-Archive -Path $zipStruct -DestinationPath $zipDestination
