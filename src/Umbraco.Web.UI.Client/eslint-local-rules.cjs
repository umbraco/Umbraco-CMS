'use strict';

const badTypeImportRule = require('./devops/eslint/rules/bad-type-import.cjs');
const enforceElementSuffixOnElementClassNameRule = require('./devops/eslint/rules/enforce-element-suffix-on-element-class-name.cjs');
const enforceUmbPrefixOnElementNameRule = require('./devops/eslint/rules/enforce-umb-prefix-on-element-name.cjs');
const enforceUmbracoExternalImportsRule = require('./devops/eslint/rules/enforce-umbraco-external-imports.cjs');
const ensureRelativeImportUseJsExtensionRule = require('./devops/eslint/rules/ensure-relative-import-use-js-extension.cjs');
const exportedStringConstantNaming = require('./devops/eslint/rules/exported-string-constant-naming.cjs');
const noDirectApiImportRule = require('./devops/eslint/rules/no-direct-api-import.cjs');
const preferImportAliasesRule = require('./devops/eslint/rules/prefer-import-aliases.cjs');
const preferStaticStylesLastRule = require('./devops/eslint/rules/prefer-static-styles-last.cjs');
const umbClassPrefixRule = require('./devops/eslint/rules/umb-class-prefix.cjs');
const noRelativeImportToImportMapModule = require('./devops/eslint/rules/no-relative-import-to-import-map-module.cjs');

module.exports = {
	'bad-type-import': badTypeImportRule,
	'enforce-element-suffix-on-element-class-name': enforceElementSuffixOnElementClassNameRule,
	'enforce-umb-prefix-on-element-name': enforceUmbPrefixOnElementNameRule,
	'enforce-umbraco-external-imports': enforceUmbracoExternalImportsRule,
	'ensure-relative-import-use-js-extension': ensureRelativeImportUseJsExtensionRule,
	'exported-string-constant-naming': exportedStringConstantNaming,
	'no-direct-api-import': noDirectApiImportRule,
	'prefer-import-aliases': preferImportAliasesRule,
	'prefer-static-styles-last': preferStaticStylesLastRule,
	'umb-class-prefix': umbClassPrefixRule,
	'no-relative-import-to-import-map-module': noRelativeImportToImportMapModule,
};
