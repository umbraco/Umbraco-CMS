[Back to Contents](1_CONTENTS.md)

# Guidelines

In this chapter:

* [Guidelines for contributions we welcome](5_GUIDELINES.md#guidelines-for-contributions-we-welcome)
* [How do I begin?](5_GUIDELINES.md#how-do-i-begin)
* [What We're Looking For](5_GUIDELINES.md#what-were-looking-for)
* [How We Decide](5_GUIDELINES.md#how-we-decide)
* [Reporting Bugs](5_GUIDELINES.md#reporting-bugs)
* [Before Submitting A Bug Report](5_GUIDELINES.md#before-submitting-a-bug-report)
* [Suggesting Enhancements](5_GUIDELINES.md#suggesting-enhancements)
* [Small Pull Requests](5_GUIDELINES.md#small-pull-requests)
* [Up for grabs](5_GUIDELINES.md#up-for-grabs)
* [Large Pull Requests](5_GUIDELINES.md#large-pull-requests)
* [Pull request or package?](5_GUIDELINES.md#pull-request-or-package)
* [Questions](5_GUIDELINES.md#questions)


## Guidelines for contributions we welcome

Not all changes are wanted so on occassion we might close a PR without merging it. We will give you feedback why we can't accept your changes and we'll be nice about it, thanking you for spending your valueable time.

We have documented what we consider small and large changes below, make sure to talk to us before making large changes.

Remember, if an issue is in the `Up for grabs` list or you've asked for some feedback before you sent us a PR, your PR will not be closed as unwanted.

## How do I begin?

Before making contributions to Umbraco - you must communicate to the Umbraco PR Team on the details of your contribution.

There are several ways of doing this depending on whether your contribution is addressing a bug or suggesting an enhancement.

Unsure where to begin contributing to Umbraco? You can start by looking through [these `Up for grabs` and issues](https://github.com/umbraco/Umbraco-CMS/issues?q=is%3Aissue+is%3Aopen+label%3Acommunity%2Fup-for-grabs).

The issue list is sorted by total number of upvotes. While not perfect, number of upvotes is a reasonable proxy for impact a given change will have.

## What We're Looking For

Not all changes are wanted so on occassion we might close a PR without merging it. We will give you feedback why we can't accept your changes and we'll be nice about it, thanking you for spending your valueable time.

We have documented what we consider small and large changes below, make sure to talk to us before making large changes.

Remember, if an issue is in the `Up for grabs` list or you've asked for some feedback before you sent us a PR, your PR will not be closed as unwanted.

Read more about `Up for grabs` [here](#up-for-grabs).

## How We Decide

When you’re considering creating a pull request for Umbraco CMS, we will categorize them in two different sizes, small and large.

The process for both sizes is very similar, as [explained in the guide on Pull Requests](#small-pull-requests).

### Reporting Bugs
This section guides you through submitting a bug report for Umbraco CMS. Following these guidelines helps maintainers and the community understand your report 📝, reproduce the behavior 💻 💻, and find related reports 🔎.

Before creating bug reports, please check the next section below [Before Submitting A Bug Report](#before-submitting-a-bug-report) as you might find out that you don't need to create one. When you are creating a bug report, please [include as many details as possible](#how-do-i-submit-a-good-bug-report). Fill out [the required template](http://issues.umbraco.org/issues#newissue=61-30118), the information it asks for helps us resolve issues faster.

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

### Before Submitting A Bug Report

  * Most importantly, check **if you can reproduce the problem** in the [latest version of Umbraco](https://our.umbraco.org/download/). We might have already fixed your particular problem.
  * It also helps tremendously to check if the issue you're experiencing is present in **a clean install** of the Umbraco version you're currently using. Custom code can have side-effects that don't occur in a clean install.
  * **Use the Google**. Whatever you're experiencing, Google it plus "Umbraco" - usually you can get some pretty good hints from the search results, including open issues and further troubleshooting hints.
  * If you do find and existing issue has **and the issue is still open**, add a comment to the existing issue if you have additional information. If you have the same problem and no new info to add, just "star" the issue.

Explain the problem and include additional details to help maintainers reproduce the problem. The following is a long description which we've boiled down into a few very simple questions in the issue tracker when you create a new issue. We're listing the following hints to indicate that the most successful reports usually have a lot of this ground covered:

  * **Use a clear and descriptive title** for the issue to identify the problem.
  * **Describe the exact steps which reproduce the problem** in as many details as possible. For example, start by explaining which steps you took in the backoffice to get to a certain undesireable result, e.g. you created a document type, inherting 3 levels deep, added a certain datatype, tried to save it and you got an error.
  * **Provide specific examples to demonstrate the steps**. If you wrote some code, try to provide a code sample as specific as possible to be able to reproduce the behavior.
  * **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
  * **Explain which behavior you expected to see instead and why.**

Provide more context by answering these questions:

  * **Can you reproduce the problem** when `debug="false"` in your `web.config` file?
  * **Did the problem start happening recently** (e.g. after updating to a new version of Umbraco) or was this always a problem?
  * **Can you reliably reproduce the issue?** If not, provide details about how often the problem happens and under which conditions it normally happens.

Include details about your configuration and environment:

  * **Which version of Umbraco are you using?** 
  * **What is the environment you're using Umbraco in?** Is this a problem on your local machine or on a server. Tell us about your configuration: Windows version, IIS/IISExpress, database type, etc.
  * **Which packages do you have installed?**

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion for Umbraco, including completely new features and minor improvements to existing functionality. Following these guidelines helps maintainers and the community understand your suggestion 📝 and find related suggestions 🔎.

Most of the suggestions in the [reporting bugs](#reporting-bugs) section also count for suggesting enhancements.

Some additional hints that may be helpful:

  * **Include screenshots and animated GIFs** which help you demonstrate the steps or point out the part of Umbraco which the suggestion is related to.
  * **Explain why this enhancement would be useful to most Umbraco users** and isn't something that can or should be implemented as a [community package](https://our.umbraco.org/projects/).

### Small Pull Requests
Bug fixes and small improvements - can be recognized by seeing a small number of changes and possibly a small number of new files.

We’re usually able to handle small PRs pretty quickly. A community volunteer will do the initial review and flag it for Umbraco HQ as “community tested”. If everything looks good, it will be merged pretty quickly [as per the described process](8_PULL_REQUESTS.md#).

### Up for grabs

Umbraco HQ will regularly mark newly created issues on the issue tracker with the `Up for grabs` tag. This means that the proposed changes are wanted in Umbraco but the HQ does not have the time to make them at this time. These issues are usually small enough to fit in the "Small PRs" category and we encourage anyone to pick them up and help out.  

If you do start working on something, make sure leave a small comment on the issue saying something like: "I'm working on this". That way other people stumbling upon the issue know they don't need to pick it up, someone already has.

To view the latest issues marked with `Up for grabs` tag then head on over to the [Umbraco issue tracker on Github](https://github.com/umbraco/Umbraco-CMS/issues?q=is%3Aissue+is%3Aopen+label%3Acommunity%2Fup-for-grabs).

### Large Pull Requests
New features and large refactorings - can be recognized by seeing a large number of changes, plenty of new files, updates to package manager files (NuGet’s packages.config, NPM’s packages.json, etc.).  

We would love to follow the same process for larger PRs but this is not always possible due to time limitations and priorities that need to be aligned. We don’t want to put up any barriers, but this document should set the correct expectations.  

Please make sure to describe your idea in an issue, it helps to put in mockup screenshots or videos.  

If the change makes sense for HQ to include in Umbraco CMS we will leave you some feedback on how we’d like to see it being implemented. 

If a larger pull request is encouraged by Umbraco HQ, the process will be similar to what is described in the [small PRs process](#small-pull-requests) above, we’ll get feedback to you within 14 days. Finalizing and merging the PR might take longer though as it will likely need to be picked up by the development team to make sure everything is in order. We’ll keep you posted on the progress.

### Pull request or package?

If it doesn’t fit in CMS right now, we will likely encourage you to make it into a package instead. A package is a great way to check out popularity of a feature, learn how people use it, validate good usability and to fix bugs.  

Eventually, a package could "graduate" to be included in the CMS.

## Questions

You can get in touch with [the PR team](#the-pr-team) in multiple ways, we love open conversations and we are a friendly bunch. No question you have is stupid. Any questions you have usually helps out multiple people with the same question. Ask away:

- If there's an existing issue on the issue tracker then that's a good place to leave questions and discuss how to start or move forward
- Unsure where to start? Did something not work as expected? Try leaving a note in the ["Contributing to Umbraco"](https://our.umbraco.org/forum/contributing-to-umbraco-cms/) forum, the team monitors that one closely
- We're also [active in the Gitter chatroom](https://gitter.im/umbraco/Umbraco-CMS)

[<< Prev ](4_QUICK_START.md)[ Next >>](CONTRIBUTING.md)
