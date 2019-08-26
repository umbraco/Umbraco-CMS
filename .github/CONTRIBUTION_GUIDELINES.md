# Contributing to Umbraco CMS

When you’re considering creating a pull request for Umbraco CMS, we will categorize them in two different sizes, small and large.

The process for both sizes is very similar, as [explained in the contribution document](CONTRIBUTING.md#how-do-i-begin).

## Small PRs
Bug fixes and small improvements - can be recognized by seeing a small number of changes and possibly a small number of new files.

We’re usually able to handle small PRs pretty quickly. A community volunteer will do the initial review and flag it for Umbraco HQ as “community tested”. If everything looks good, it will be merged pretty quickly [as per the described process](REVIEW_PROCESS.md).

### Up for grabs

Umbraco HQ will regularly mark newly created issues on the issue tracker with the `Up for grabs` tag. This means that the proposed changes are wanted in Umbraco but the HQ does not have the time to make them at this time. These issues are usually small enough to fit in the "Small PRs" category and we encourage anyone to pick them up and help out.  

If you do start working on something, make sure leave a small comment on the issue saying something like: "I'm working on this". That way other people stumbling upon the issue know they don't need to pick it up, someone already has.

## Large PRs
New features and large refactorings - can be recognized by seeing a large number of changes, plenty of new files, updates to package manager files (NuGet’s packages.config, NPM’s packages.json, etc.).  

We would love to follow the same process for larger PRs but this is not always possible due to time limitations and priorities that need to be aligned. We don’t want to put up any barriers, but this document should set the correct expectations.  

Please make sure to describe your idea in an issue, it helps to put in mockup screenshots or videos.  

If the change makes sense for HQ to include in Umbraco CMS we will leave you some feedback on how we’d like to see it being implemented. 

If a larger pull request is encouraged by Umbraco HQ, the process will be similar to what is described in the [small PRs process](#small-prs) above, we’ll get feedback to you within 14 days. Finalizing and merging the PR might take longer though as it will likely need to be picked up by the development team to make sure everything is in order. We’ll keep you posted on the progress.

### Pull request or package?

If it doesn’t fit in CMS right now, we will likely encourage you to make it into a package instead. A package is a great way to check out popularity of a feature, learn how people use it, validate good usability and to fix bugs.  

Eventually, a package could "graduate" to be included in the CMS.  