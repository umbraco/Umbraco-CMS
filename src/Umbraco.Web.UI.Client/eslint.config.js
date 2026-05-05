// For more info, see https://github.com/storybookjs/eslint-plugin-storybook#configuration-flat-config-format

import js from '@eslint/js';
import globals from 'globals';
import importPlugin from 'eslint-plugin-import';
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
			'import/namespace': 'off',
			'import/no-unresolved': 'off',
			'import/order': ['warn', { groups: ['builtin', 'parent', 'sibling', 'index', 'external'] }],
			'import/no-self-import': 'error',
			'import/no-cycle': ['error', { maxDepth: 6, allowUnsafeDynamicCyclicDependency: true }],
			'local-rules/prefer-static-styles-last': 'warn',
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
					definedTags: ['element', 'attr', 'fires', 'prop', 'slot', 'cssprop', 'csspart'],
				},
			],
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
		files: ['**/*.js'],
		...tseslint.configs.disableTypeChecked,
		languageOptions: {
			globals: {
				...globals.node,
			},
		},
	},
];
