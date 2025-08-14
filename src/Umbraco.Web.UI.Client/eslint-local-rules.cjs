'use strict';

const enforceElementSuffixOnElementClassNameRule = require('./devops/eslint/rules/enforce-element-suffix-on-element-class-name.cjs');
const enforceUmbracoExternalImportsRule = require('./devops/eslint/rules/enforce-umbraco-external-imports.cjs');
const ensureRelativeImportUseJsExtensionRule = require('./devops/eslint/rules/ensure-relative-import-use-js-extension.cjs');
const noDirectApiImportRule = require('./devops/eslint/rules/no-direct-api-import.cjs');
const preferImportAliasesRule = require('./devops/eslint/rules/prefer-import-aliases.cjs');
const preferStaticStylesLastRule = require('./devops/eslint/rules/prefer-static-styles-last.cjs');
const noRelativeImportToImportMapModule = require('./devops/eslint/rules/no-relative-import-to-import-map-module.cjs');

module.exports = {
	'enforce-element-suffix-on-element-class-name': enforceElementSuffixOnElementClassNameRule,
	'enforce-umbraco-external-imports': enforceUmbracoExternalImportsRule,
	'ensure-relative-import-use-js-extension': ensureRelativeImportUseJsExtensionRule,
	'no-direct-api-import': noDirectApiImportRule,
	'prefer-import-aliases': preferImportAliasesRule,
	'prefer-static-styles-last': preferStaticStylesLastRule,
	'no-relative-import-to-import-map-module': noRelativeImportToImportMapModule,
};
