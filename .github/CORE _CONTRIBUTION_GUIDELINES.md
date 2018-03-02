# Guidelines for Core Contribution

This document contains the proposed plan of action for how to approach involvement of the community and the continued development of the Umbraco v4 core.

## Roles in the HQ

The roles occupied by HQ staff members are as follows:

**Umbraco Core:**
- Shannon Deminick
- Sebastiaan Janssen
- Mads Rasmussen
- Rune Strand
- Simon Busborg
- Claus Jensen
- Stéphane Gay

**Product Owner:**
- Niels Hartvig

**Scrum master / Project Manager:**
- Sebastiaan Janssen

**Documentation Project Owner:**
- Rune Strand

## Core team process

Umbraco HQ uses the Scrum process in order to establish a fixed team that can build up a velocity helping us improve as a development team - both in regards to the amount of tasks (story points) that can be completed within a Sprint and with regards to planning a release (Sprint).

The idea is to have 4 x two week Sprints that over a 8 week period results in a release - either Major or Minor release. The type of release shouldn't matter as ultimately it is about having a deliverable as the end result. At the end of each Sprint will be a working product, which combined makes up a deliverable (eg. a release).
Patch releases will appear every 4 weeks and will only contain bug fixes, a patch released is planned for a certain date but if there's a pressing need to do a release then this can happen at any time. The next patch release will then be scheduled for 4 weeks after that again. 

The roles include a Product Owner, Scrum Master and Scrum team(s).

The Product Owner will create a backlog of prioritized items, which are pulled into the Sprints during planning. Seeing as we are creating a long term road map, some tasks will be predefined, but it's important to note that the scrum team has to commit to these tasks before the road map is valid.

The road map might contain loosely defined tasks, which need to be made concrete for a given Sprint. It might also be necessary to pull in additional tasks if the road map does not contain enough tasks for the team to work on.

The planning process will occur on a monthly basis after each release.

The backlog of items/tasks are open to both the Scrum team(s) and outside contributors. Before tasks can be pulled from the backlog to a Sprint it should be approved by the Scrum master, as they are the one with the overview.

There will be one fixed Scrum team consisting of HQ Core developers that will be the main drivers of the road map. Aside from this, contributors can form their own teams and work on tasks (features, bug fixes etc) as agreed upon with the Scrum master.

Contributors can of course also work individually and their contributions (pull requests) will be assessed and if accepted merged according to how it fits the current plan (road map) and Sprint log.

To the extent it's possible the Product Owner should write User Stories for new features, whereas bug fixes and other smaller tasks will remain a simple description of the task at hand.

The team will gradually start to add story points to the tasks in a Sprint during Sprint planning, so we can start recording the fixed team's velocity. This will however take some time to implement, so we should not stress to spend time on writing user stories and assigning story points - this is a process that will evolve over time.

## Adoption of SemVer (www.semver.org)

Versioning is an important aspect of the project as it should clearly communicate with the community information about each release and where each release fits on the road map. The easy and obvious choice is to use Semantic Versioning, which is a well defined set of rules for when to change version number based major, minor and patch changes.

Using this semantic versioning will also help in terms of the road map, as it will be clear when there is a need to change major and minor numbers based on the tasks being implemented for a specific release. Below is the two most important rules, which should be considered when planning the road map.

"Minor version Y (x.Y.z | x > 0) MUST be incremented if new, backwards compatible functionality is introduced to the public API. It MUST be incremented if any public API functionality is marked as deprecated. It MAY be incremented if substantial new functionality or improvements are introduced within the private code. It MAY include patch level changes. Patch version MUST be reset to 0 when minor version is incremented."

"Major version X (X.y.z | X > 0) MUST be incremented if any backwards incompatible changes are introduced to the public API. It MAY include minor and patch level changes. Patch and minor version MUST be reset to 0 when major version is incremented."

Semantic versioning will also be used for nuget packages, so we can provide pre-release packages for testing - ie. Umbraco.4.8.0-beta.nupkg.

## Approving features and breaking changes - RFC

Whenever the Core team or contributors want to implement a new "larger" feature or introduce breaking changes, a 'request for comments' should be submitted to the Core mailing list detailing the proposed changes and/or additions.

**An RFC should at best and to the extent of the proposed changes contain the following:**

- Documentation
  - Why should the proposed changes be implemented in the Core?
  - How should it be integrated?
  - What are the consequences of the proposed changes - will it break or deprecate anything meaning major or minor release?
- Proof of concept or pseudo code if it makes sense
- Await feedback and assess whether to go ahead or rethink the proposed changes.

## Working with branches

Each major release (4, 6, 7, etc...) has 2 branches: 'master-x' and 'dev-x' named according to the major version. (i.e. master-v7 and dev-v7).

The 'master-x' branch is the baseline branch for a given major version and is the stable version of the current release. The 'dev-x' branch is the current development branch for a given major version. Any time a new version of Umbraco is released, the 'dev-x' branch will be tagged at the revision of the release with the release name and merged in to the 'master-x' branch.

If you are creating pull requests, you should do so on the 'dev-'x branch. If you want to work with the latest stable released code, you should clone the 'master-x' branch.

New features for Umbraco will normally be done on a feature/WIP (work in progress) branch that will be branched from the 'dev-x' branch and merged back in when that feature is complete. Feature branches should be named accordingly and prefixed with the 'dev-x' branch it was created from. i.e. dev-v7-LocalizationService

When working on "normal" tasks or bug fixes, they should be committed to the 'dev-x' branch along with clear notes in the comments about the task or bug that the commit address'. ie. 

Fixes: U4-97 Enable RTE label as default

All developers should commit early, commit often and make sure to push changes to the remote repository - inspired by http://www.codinghorror.com/blog/2008/08/check-in-early-check-in-often.html

When working on features or bug fixes in a fork the targeted branch should be used. Eg. when working on a feature for version 7.2.0 then commit changes to that branch. Also make sure to get the fork up-to-date before submitting a pull request, so that merging conflicts and the possibility that the pull request is rejected can be avoided.

## Contributing to the Umbraco Core

Direct commit access to the source code repository is limited to the Core team. This rule is enforced to ensure the stability of the core and to ensure that velocity is maintained and releases are not affected by code containing errors. Contributors should fork the project and send patches or pull requests, which will be reviewed by the Scrum master and Core team before being committed to the main branch/repository.

As Contributors and Contribution teams start to learn the development workflow they may be granted access to commit to the main repository, and will be governed by the same guidelines for working with branches as the Core team.

**The flow for contributing new features or bug fixes is as follows:**

1. Contact the Scrum master / Project manager from Umbraco HQ with a note about what you wish to work on.
2. Once agreement on when this addition/improvement fits in, fork the repository and start working.
3. Work is implemented in the branch that corresponds to the release that the work is targeted at.
4. When work is complete, update the fork from the main branch and ensure all code is working and tested. Once satisfied send a patch / pull request.
5. Once the contribution has been reviewed by the Core team it will either be merged into the main repository or rejected with reasoning.

All contributors should consider writing unit tests for the code they are implementing - whenever it makes sense. It is not a must to write unit tests, but it is encouraged for new features and bug fixes that are not "100%" UI centric.

Umbraco fully respects that contributors have limited time, and might not be able to work on features or bug fixes within a fixed timeframe. We will therefore be as flexible as possible with regards to the above flow of contributing.

## (Naming) Conventions

When developing new Class Libraries we will be adhereing as closely as possible to the official guidelines as proposed by Microsoft -http://msdn.microsoft.com/en-us/library/ms229042.aspx

Another good reference is "Framework Design Guidelines: Conventions, Idioms, and Patterns for Reusable .NET Libraries" book by Krzysztof Cwalina and Brad Abrams

Resharper settings are included with the solution, so developers can cleanup code in a consistent manner.

A special note about `whitespace` is in order. Usage should follow Visual Studio defaults which is **4 spaces** for pretty much everything except for XML, which uses **2 spaces** instead (again, this is the Visual Studio defaults).

## Reviews of code contributions

These are the things that will be looked at before your patch/pull request can be accepted.

- Is there a Issue ID attached?
- For non-major releases:
  - The workitem shouldn't introduce breaking API changes (results of API call changes, method signature changes)
  - Any large behavioral changes should be avoided for anything except for major release
- Are the coding standards followed?
  - [Coding standards guidelines documentation](https://our.umbraco.org/documentation/Development-Guidelines/Coding-Standards/)
- Does the code compile and work? 
  - Unit tests to verify that the code works
  - Otherwise: communicate, make sure the reviewer knows what the code is supposed to do
- Does it add value?
  - For example: is this not solving something that might be slated for a release in the near future (i.e. should we spend time on this now if it's going to change within 2 months?)
  - But also: is this really something we want in the core? If you're not sure, before you start writing code, make sure to post to the core dev mailing list to see if you're on the right path 
- Any security implications? Think about:
  - Data Validation
  - Authentication
  - Session Management
  - Authorization
  - Cryptography
  - Consult OWASP when in doubt

## Release naming conventions

Umbraco releases should be named as follows (according to SemVer) for consistency.

**Source code repository:**
- UmbracoCms.7.7.0.zip
- UmbracoCms.7.7.0-beta.zip (for pre-release)
- UmbracoCms.Source.7.7.0.zip
- UmbracoCms.WebPI.7.7.0.zip

**Nightlies:**
- UmbracoCms.7.7.0-build.297 (where 297 is the build number - AssemblyVersion 7.7.0.297)

**Nuget:**
- UmbracoCms.7.7.0.nupkg
- UmbracoCms.7.7.0-beta.nupkg (for pre-release)
- UmbracoCms.Core.7.7.0.nupkg
- UmbracoCms.SqlCE.7.7.0.nupkg