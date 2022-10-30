# Contribute
Want to help make Sa11y better? There are many ways you can contribute.

- Report a bug.
- Submit a bug fix.
- Propose new features or rulesets.
- Discuss existing rulesets. 
- Help translate into other languages.
- Provide feedback or a testimonial!

## How to report bugs
Report bugs by creating a new [issue.](https://github.com/ryersondmp/sa11y/issues) If you do not have a GitHub account, feel free to email [adam.chaboryk@ryerson.ca](mailto:adam.chaboryk@ryerson.ca) 

When creating an issue, try to: 
- Provide a detailed summary of the issue and screenshot if possible.
- If possible, provide the URL where Sa11y didn't work properly. 
- Provide specific steps to reproduce the issue.

## How to propose new rulesets or discuss an existing one
Some of Sa11y's rulesets are based on best practices or are inspired by commonly seen issues that were created by content authors at Ryerson University. Therefore some rulesets may contain logic that uses arbitrary variables that are not based on normative WCAG success criteria or failures. For example, if a blockquote contains less than 25 characters it will be flagged as an error. This is not a WCAG failure. However, if a content author is using a blockquote as a section heading for visual aesthetics, then it does become a failure of 1.3.1: Info and Relationships. Using an arbitrary number like 25, we assume that it is a short section heading and not a real quote. 

If you would like to discuss an existing ruleset, please create a new [issue.](https://github.com/ryersondmp/sa11y/issues) Provide any references or sources if you are proposing a new ruleset or would like to change an existing one.

## Translations
Translations may either be contributed back to the repository with a pull request, or translated files can be returned to: [adam.chaboryk@ryerson.ca](mailto:adam.chaboryk)

## How to contribute code
There's a couple of ways you can contribute code.

### Option 1: Create an issue
If it's a simple code fix or ruleset: consider creating a new [issue.](https://github.com/ryersondmp/sa11y/issues) Please fork the repo and test your code before submitting it.

### Option 2: Pull request
Pull requests should be used for “heavy-duty” changes to the code base. Although before you invest time and effort, please create an [issue](https://github.com/ryersondmp/sa11y/issues) discussing your idea. Once approved, feel free to go ahead and fork the repo. 

1. Fork the repo and create your branch from `master`.
2. Add comments to your code.
3. Thoroughly test your code.
4. Make sure your code lints.
5. Make sure your code does not introduce any accessibility barriers.
6. Create a pull request!

## License
By contributing, you agree that your contributions will be licensed under the same [license as Sa11y.](https://github.com/ryersondmp/sa11y/blob/master/LICENSE.md)
