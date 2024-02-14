## Unwanted changes
While most changes are welcome, there are certain types of changes that are discouraged and might get your pull request refused.
Of course this will depend heavily on the specific change, but please take the following examples in mind.

- **Breaking changes (code and/or behavioral) 💥** - sometimes it can be a bit hard to know if a change is breaking or not. Fortunately, if it relates to code, the build will fail and warn you.
- **Large refactors 🤯** - the larger the refactor, the larger the probability of introducing new bugs/issues.  
- **Changes to obsolete code and/or property editors ✍️**
- **Adding new config options 🦾** - while having more flexibility is (most of the times) better, having too many options can also become overwhelming/confusing, especially if there are other (good/simple) ways to achieve it.
- **Whitespace changes 🫥** - while some of our files might not follow the formatting/whitespace rules (mostly old ones), changing several of them in one go would cause major merge conflicts with open pull requests or other work in progress. Do feel free to fix these when you are working on another issue/feature and end up "touching" those files!
- **Adding new extension/helper methods ✋** - keep in mind that more code also means more to maintain, so if a helper is only meaningful for a few, it might not be worth adding it to the core.

While these are only a few examples, it is important to ask yourself these questions before making a pull request:

- How many will benefit from this change?
- Are there other ways to achieve this? And if so, how do they compare?
- How maintainable is the change?
- What would be the effort to test it properly?
- Do the benefits outweigh the risks?