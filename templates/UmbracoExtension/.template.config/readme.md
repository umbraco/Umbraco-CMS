# Customising the umbraco-extension template

## Source Name

The source name is set to `Umbraco.Extension`

The templating engine will rename any folder or file whose name contains `Umbraco.Extension` replacing it with the provided name. 

The templating engine will replace the text in any file as follows:

- `Umbraco.Extension` with the safe namespace for the provided name
- `Umbraco_Extension` with the safe default class name for the provided name
- `umbraco.extension` with the safe namespace for the provided name, in lower case
- `umbraco_extension` with the safe default class name for the provided name, in lower case

## Custom Replacements

The following custom placeholders have been configured in `template.json`:

- `UmbracoExtension` will be replaced with the safe namespace but without . or _
- `umbracoextension` will be replaced with the safe namespace but without . or _ , in lower case
- `umbraco-extension` will be replaced with the kebab case transform of the provided name
- `Umbraco Extension` will be replaced with a 'friendly' version of the provided name, e.g. MyProject > My Project. NB it will render a trailing space so you don't need to add one.

The first three custom placeholders have been configured to replace the text in both files and filenames.