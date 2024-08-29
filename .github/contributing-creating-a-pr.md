## Creating a pull request

Exciting! You're ready to show us your changes.

We recommend you to [sync with our repository][sync fork] before you submit your pull request. That way, you can fix any potential merge conflicts and make our lives a little bit easier.

GitHub will have picked up on the new branch you've pushed and will offer to create a Pull Request. Click that green button and away you go.
![Create a pull request](img/createpullrequest.png)

We like to use [git flow][git flow] as much as possible, but don't worry if you are not familiar with it. The most important thing you need to know is that when you fork the Umbraco repository, the default branch is set to `contrib`. This is the branch you should be targeting.

Please note: we are no longer accepting features for v8 and below but will continue to merge security fixes as and when they arise.
  
## The review process
[review process]: #the-review-process

You've sent us your contribution - congratulations! Now what?

The [Core Collaborators team][Core collabs] can now start reviewing your proposed changes and give you feedback on them. If it's not perfect, we'll either fix up what we need or we can request that you make some additional changes.

You will get an initial automated reply from our [Friendly Umbraco Robot, Umbrabot][Umbrabot], to acknowledge that we’ve seen your PR and we’ll pick it up as soon as we can. You can take this opportunity to double check everything is in order based off the handy checklist Umbrabot provides.

You will get feedback as soon as the [Core Collaborators team][Core collabs] can after opening the PR. You’ll most likely get feedback within a couple of weeks. Then there are a few possible outcomes:

- Your proposed change is awesome! We merge it in and it will be included in the next minor release of Umbraco
- If the change is a high priority bug fix, we will cherry-pick it into the next patch release as well so that we can release it as soon as possible
- Your proposed change is awesome but needs a bit more work, we’ll give you feedback on the changes we’d like to see
- Your proposed change is awesome but... not something we’re looking to include at this point. We’ll close your PR and the related issue (we’ll be nice about it!). See [making larger changes][making larger changes] and [pull request or package?][pr or package]

### Dealing with requested changes

If you make the corrections we ask for in the same branch and push them to your fork again, the pull request automatically updates with the additional commit(s) so we can review it again. If all is well, we'll merge the code and your commits are forever part of Umbraco!

#### No longer available?

We understand you have other things to do and can't just drop everything to help us out.

So if we’re asking for your help to improve the PR we’ll wait for two weeks to give you a fair chance to make changes. We’ll ask for an update if we don’t hear back from you after that time.  

If we don’t hear back from you for 4 weeks, we’ll close the PR so that it doesn’t just hang around forever. You’re very welcome to re-open it once you have some more time to spend on it.  

There will be times that we really like your proposed changes and we’ll finish the final improvements we’d like to see ourselves. You still get the credits and your commits will live on in the git repository.


[ Umbrabot ]: https://github.com/umbrabot
[git flow]: https://jeffkreeftmeijer.com/git-flow/	"An explanation of git flow"


[making larger changes]: contributing-before-you-start.md#making-large-changes
[pr or package]: contributing-before-you-start.md#pull-request-or-package
[Core collabs]: contributing-core-collabs-team.md