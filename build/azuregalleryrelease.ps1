Param(
 [string]$GitHubPersonalAccessToken,
 [string]$Directory
)
$workingDirectory = $Directory
CD "$($workingDirectory)"

# Clone repo
$fullGitUrl = "https://$($env:GIT_URL)/$($env:GIT_REPOSITORYNAME).git"
git clone $($fullGitUrl) $($env:GIT_REPOSITORYNAME) 2>&1 | % { $_.ToString() } 

# Remove everything so that unzipping the release later will update everything
# Don't remove the readme file nor the git directory
Write-Host "Cleaning up git directory before adding new version"
Remove-Item -Recurse "$($workingDirectory)\$($env:GIT_REPOSITORYNAME)\*" -Exclude README.md,.git

# Find release zip
$zipsDir = "$($workingDirectory)\$($env:BUILD_DEFINITIONNAME)\zips"
$pattern = "UmbracoCms.([0-9]{1,2}.[0-9]{1,3}.[0-9]{1,3}).zip"
Write-Host "Searching for Umbraco release files in $($zipsDir) for a file with pattern $($pattern)"
$file = (Get-ChildItem "$($zipsDir)" | Where-Object { $_.Name -match "$($pattern)" })

if($file)
{
    # Get release name
    $version = [regex]::Match($($file.Name), $($pattern)).captures.groups[1].value
    $releaseName = "Umbraco $($version)"
    Write-Host "Found $($releaseName)"
    
    # Unzip into repository to update release
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    Write-Host "Unzipping $($file.FullName) to $($workingDirectory)\$($env:GIT_REPOSITORYNAME)"
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$($file.FullName)", "$($workingDirectory)\$($env:GIT_REPOSITORYNAME)")
    
    # Telling git who we are
    git config --global user.email "coffee@umbraco.com" 2>&1 | % { $_.ToString() }
    git config --global user.name "Umbraco HQ" 2>&1 | % { $_.ToString() }
    
    # Commit
    CD "$($workingDirectory)\$($env:GIT_REPOSITORYNAME)"
    Write-Host "Committing Umbraco $($version) Release from Build Output"

    git add . 2>&1 | % { $_.ToString() } 
    git commit -m " Release $($releaseName) from Build Output" 2>&1 | % { $_.ToString() } 

    # Tag the release
    git tag -a "v$($version)" -m "v$($version)"

    # Push release to master
    $fullGitAuthUrl = "https://$($env:GIT_USERNAME):$($GitHubPersonalAccessToken)@$($env:GIT_URL)/$($env:GIT_REPOSITORYNAME).git"
    git push $($fullGitAuthUrl) 2>&1 | % { $_.ToString() } 

    #Push tag to master
    git push $($fullGitAuthUrl) --tags 2>&1 | % { $_.ToString() }
}
else
{
    Write-Error "Umbraco release file not found, searched in $($workingDirectory)\$($zipsDir) for a file with pattern $($pattern) - canceling"
}
