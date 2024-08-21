# Bellissima release instructions


## Build

> _See internal documentation on the build/release workflow._


## GitHub Release Notes

To generate release notes on GitHub.

1. Go to the [**Releases** area](https://github.com/umbraco/Umbraco.CMS.Backoffice/releases)
2. Press the [**"Draft a new release"** button](https://github.com/umbraco/Umbraco.CMS.Backoffice/releases/new)
3. In the combobox for "Choose a tag", expand then select or enter the next version number, e.g. `v14.2.0`
  - If the tag does not already exist, an option labelled "Create new tag: v14.2.0 on publish" will appear, select that
4. In the combobox for "Target: main", expand then select the release branch for the next version, e.g. `release/14.2`
5. In the combobox for "Previous tag: auto":
  - If the next release is an RC, then you can leave as `auto`
	- Otherwise, select the previous stable version, e.g. `v14.1.1`
6. Press the **"Generate release notes"** button, this will populate the main textarea
7. Check the details, view in the "Preview" tab
8. What type of release is this?
  - If it's an RC, then check "Set as a pre-release"
	- If it's stable, then check "Set as the latest release"
9. Once you're happy with the contents and ready to save...
  - If you need more time to review, press the **"Save draft"** button and you can come back to it later	
  - If you are ready to make the release notes public, then press **"Publish release"** button! :tada:

> If you're curious about how the content is generated, take a look at the `release.yml` configuration: 
> https://github.com/umbraco/Umbraco.CMS.Backoffice/blob/main/.github/release.yml
