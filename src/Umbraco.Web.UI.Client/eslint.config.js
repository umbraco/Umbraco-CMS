// For more info, see https://github.com/storybookjs/eslint-plugin-storybook#configuration-flat-config-format

import js from '@eslint/js';
import globals from 'globals';
import importPlugin from 'eslint-plugin-import-x';
import localRules from 'eslint-plugin-local-rules';
import storybook from 'eslint-plugin-storybook';
import wcPlugin from 'eslint-plugin-wc';
import litPlugin from 'eslint-plugin-lit';
import eslintPluginPrettierRecommended from 'eslint-plugin-prettier/recommended';
import tseslint from 'typescript-eslint';
import jsdoc from 'eslint-plugin-jsdoc';

export default [
	// Recommended config applied to all files
	js.configs.recommended,
	importPlugin.flatConfigs.recommended,
	...tseslint.configs.recommended,
	wcPlugin.configs['flat/recommended'],
	litPlugin.configs['flat/recommended'], // We use the non typescript version to allow types to be defined in the jsdoc comments. This will allow js docs as an alternative to typescript types.
	jsdoc.configs['flat/recommended'],
	...storybook.configs['flat/recommended'],
	localRules.configs.all,
	eslintPluginPrettierRecommended,

	// Global ignores
	{
		ignores: [
			'.storybook',
			'**/eslint.config.js',
			'**/rollup.config.js',
			'**/vite.config.ts',
			'src/external',
			'src/packages/core/icon-registry/icons',
			'src/packages/core/icon-registry/icons.ts',
			'src/**/*.test.ts',
			'src/packages/core/backend-api',
			'src/packages/core/openapi-ts.*.js',
		],
	},

	// Global config
	{
		plugins: {
			'local-rules': localRules,
		},
		rules: {
			semi: ['warn', 'always'],
			'prettier/prettier': ['warn', { endOfLine: 'auto' }],
			'no-var': 'error',
			'import-x/namespace': 'off',
			'import-x/no-unresolved': 'off',
			// Off: this codebase deliberately gives many classes/elements both a named export and a
			// `export default` (the default is required for manifest-driven dynamic `js: () => import(...)`
			// loading). The rule then flags every plain `import Foo from './foo.js'` of that class elsewhere,
			// forcing `import { Foo } from './foo.js'` for no behavioural benefit.
			'import-x/no-named-as-default': 'off',
			// Off: false-positives on barrel files that reach the same underlying binding via two `export *`
			// paths (e.g. an index.ts re-exporting both a submodule barrel and that submodule's constants
			// directly) — harmless per the ES module spec, but the rule doesn't resolve to the original
			// declaration before comparing names.
			'import-x/export': 'off',
			'import-x/order': ['warn', { groups: ['builtin', 'parent', 'sibling', 'index', 'external'] }],
			'import-x/no-self-import': 'error',
			'import-x/no-cycle': ['error', { maxDepth: 6, allowUnsafeDynamicCyclicDependency: true }],
			'local-rules/enforce-manifest-alias': 'warn',
			'local-rules/prefer-static-styles-last': 'warn',
			'local-rules/no-unsafe-localize': 'error',
			'local-rules/no-unknown-localization-key': 'error',
			'local-rules/enforce-null-observe-alias-in-constructor': 'error',
			'local-rules/enforce-umbraco-external-imports': [
				'error',
				{
					exceptions: ['@umbraco-cms', '@open-wc/testing', '@storybook', 'msw', '.', 'vite', 'uuid', 'diff'],
				},
			],
			'jsdoc/check-tag-names': [
				'warn',
				{
					// allow all tags from https://github.com/runem/web-component-analyzer
					// plus the schema-annotation tags used by `typescript-json-schema` on manifest/extension
					// type definitions (see https://github.com/YousefED/typescript-json-schema) and a couple
					// of informal tags (`optional`, `observable`, `note`) already established in this codebase
					definedTags: [
						'element',
						'attr',
						'fires',
						'prop',
						'slot',
						'cssprop',
						'csspart',
						'title',
						'examples',
						'required',
						'minProperties',
						'optional',
						'observable',
						'note',
						'TJS-type',
						'TJS-ignore',
					],
				},
			],
		},
		settings: {
			jsdoc: {
				structuredTags: {
					// Web-component custom events are documented as `@fires {EventType} name - description`
					// (per web-component-analyzer) and are often kebab-case (e.g. `slice-update`), which the
					// default "namepath" name role rejects — relax both the type and name checks.
					fires: { name: 'text', type: true, required: ['name'] },
					event: { name: 'text' },
				},
			},
		},
	},

	// Pattern-specific overrides
	{
		files: ['**/*.ts'],
		ignores: ['.storybook', '**/*.stories.ts', '**/umbraco-package.ts', 'src/assets/lang/*.ts'],
		languageOptions: {
			parserOptions: {
				project: true,
				tsconfigRootDir: import.meta.dirname,
			},
			globals: {
				...globals.browser,
			},
		},
		...importPlugin.flatConfigs.typescript,
		rules: {
			// import-x/named is off in the plugin's own typescript preset (spread above), but that preset's
			// `rules` is a sibling key here and gets overwritten by this literal — restate it explicitly.
			// TS type-checking already covers "does this import exist"; the rule's own resolution is prone
			// to false negatives on deep/cyclic re-export barrels.
			'import-x/named': 'off',
			'no-unused-vars': 'off', //Let '@typescript-eslint/no-unused-vars' catch the errors to allow unused function parameters (ex: in interfaces)
			'@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
			'@typescript-eslint/no-non-null-assertion': 'off',
			'@typescript-eslint/no-explicit-any': 'warn',
			'@typescript-eslint/consistent-type-exports': 'error',
			'@typescript-eslint/consistent-type-imports': 'error',
			'@typescript-eslint/no-import-type-side-effects': 'warn',
			'@typescript-eslint/no-deprecated': 'warn',
			'@typescript-eslint/naming-convention': [
				'error',
				// All private members should be camelCase with leading underscore
				// This is to ensure that private members are not used outside the class, as they
				// are not part of the public API.
				// Example NOT OK: private myPrivateVariable
				// Example OK: private _myPrivateVariable
				{
					selector: 'memberLike',
					modifiers: ['private'],
					format: ['camelCase'],
					leadingUnderscore: 'require',
					trailingUnderscore: 'forbid',
				},
				// All public members and variables should be camelCase without leading underscore
				// Example: myPublicVariable, myPublicMethod
				{
					selector: ['variableLike', 'memberLike'],
					modifiers: ['public'],
					filter: {
						regex: '^_host$',
						match: false,
					},
					format: ['camelCase', 'UPPER_CASE', 'PascalCase'],
					leadingUnderscore: 'allowDouble',
					trailingUnderscore: 'forbid',
				},
				// All #private members and variables should be camelCase without leading underscore
				// Example: #myPublicVariable, #myPublicMethod
				{
					selector: ['variableLike', 'memberLike'],
					modifiers: ['#private'],
					format: ['camelCase', 'UPPER_CASE', 'PascalCase'],
					leadingUnderscore: 'allowDouble',
					trailingUnderscore: 'forbid',
				},
				// All protected members and variables should be camelCase with optional leading underscore (if needed to be pseudo-private)
				// Example: protected myPublicVariable, protected _myPublicMethod
				{
					selector: ['variableLike', 'memberLike'],
					modifiers: ['protected'],
					format: ['camelCase'],
					leadingUnderscore: 'allow',
					trailingUnderscore: 'forbid',
				},
				// Allow quoted properties, as they are often used in JSON or when the property name is not a valid identifier
				// This is to ensure that properties can be used in JSON or when the property name
				// is not a valid identifier (e.g. contains spaces or special characters)
				// Example: { "umb-some-component": UmbSomeComponent }
				{
					selector: ['objectLiteralProperty', 'typeProperty', 'enumMember'],
					modifiers: ['requiresQuotes'],
					format: null,
				},
				// All (exported) types should be PascalCase with leading 'Umb' or 'Example'
				// Example: UmbExampleType, ExampleTypeLike
				{
					selector: 'typeLike',
					modifiers: ['exported'],
					format: ['PascalCase'],
					prefix: ['Umb', 'Ufm', 'Manifest', 'Meta', 'Example'],
				},
				// All exported string constants should be UPPER_CASE with leading 'UMB_'
				// Example: UMB_EXAMPLE_CONSTANT
				{
					selector: 'variable',
					modifiers: ['exported', 'const'],
					types: ['string', 'number', 'boolean'],
					format: ['UPPER_CASE'],
					prefix: ['UMB_'],
				},
				// Allow destructured variables to be named as they are in the object
				{
					selector: 'variable',
					modifiers: ['destructured'],
					format: null,
				},
			],
		},
	},
	{
		// Localization dictionaries aren't type-checked (see the `ignores` above), so
		// `enforce-manifest-alias`'s type-aware manifest detection can't run here and it falls back to a
		// structural check (any object with sibling `alias`/`type` keys). Several translation entries happen
		// to have both keys (e.g. a "Type" label and an "Alias" label in the same dictionary section),
		// which the fallback misreads as unaliased manifests.
		files: ['src/assets/lang/*.ts'],
		rules: {
			'local-rules/enforce-manifest-alias': 'off',
		},
	},
	{
		// This file is annotated for `typescript-json-schema` (https://github.com/YousefED/typescript-json-schema),
		// whose `@examples` values are multi-line JSON literals — valid for the schema generator, but the
		// jsdoc comment parser reads the array/object brackets as an (invalid) tag name spanning lines.
		files: ['src/json-schema/**/*.ts'],
		rules: {
			'jsdoc/valid-types': 'off',
		},
	},
	{
		files: ['**/*.js'],
		...tseslint.configs.disableTypeChecked,
		languageOptions: {
			globals: {
				...globals.node,
			},
		},
	},
];
