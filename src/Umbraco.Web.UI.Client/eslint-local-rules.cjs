'use strict';

const badTypeImportRule = require('./devops/eslint/rules/bad-type-import.cjs');
const enforceElementSuffixOnElementClassNameRule = require('./devops/eslint/rules/enforce-element-suffix-on-element-class-name.cjs');
const enforceUmbracoExternalImportsRule = require('./devops/eslint/rules/enforce-umbraco-external-imports.cjs');
const ensureRelativeImportUseJsExtensionRule = require('./devops/eslint/rules/ensure-relative-import-use-js-extension.cjs');
const noDirectApiImportRule = require('./devops/eslint/rules/no-direct-api-import.cjs');
const preferImportAliasesRule = require('./devops/eslint/rules/prefer-import-aliases.cjs');
const preferStaticStylesLastRule = require('./devops/eslint/rules/prefer-static-styles-last.cjs');
const umbClassPrefixRule = require('./devops/eslint/rules/umb-class-prefix.cjs');

module.exports = {
	'bad-type-import': badTypeImportRule,
	'enforce-element-suffix-on-element-class-name': enforceElementSuffixOnElementClassNameRule,
	'enforce-umbraco-external-imports': enforceUmbracoExternalImportsRule,
	'ensure-relative-import-use-js-extension': ensureRelativeImportUseJsExtensionRule,
	'no-direct-api-import': noDirectApiImportRule,
	'prefer-import-aliases': preferImportAliasesRule,
	'prefer-static-styles-last': preferStaticStylesLastRule,
	'umb-class-prefix': umbClassPrefixRule,
	'prefix-exported-constants': require('./devops/eslint/rules/prefix-exported-const.cjs'),
};
