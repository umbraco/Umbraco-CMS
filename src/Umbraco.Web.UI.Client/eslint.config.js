import js from '@eslint/js';
import globals from 'globals';
import importPlugin from 'eslint-plugin-import';
import localRules from 'eslint-plugin-local-rules';
import wcPlugin from 'eslint-plugin-wc';
import litPlugin from 'eslint-plugin-lit';
import eslintPluginPrettierRecommended from 'eslint-plugin-prettier/recommended';
import tseslint from 'typescript-eslint';
import jsdoc from 'eslint-plugin-jsdoc';

export default [
	// Recommended config applied to all files
	js.configs.recommended,
	...tseslint.configs.recommended,
	wcPlugin.configs['flat/recommended'],
	litPlugin.configs['flat/recommended'],
	jsdoc.configs['flat/recommended'], // We use the non typescript version to allow types to be defined in the jsdoc comments. This will allow js docs as an alternative to typescript types.
	localRules.configs.all,
	eslintPluginPrettierRecommended,

	// Global ignores
	{
		ignores: [
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
		languageOptions: {
			parserOptions: {
				project: true,
				tsconfigRootDir: import.meta.dirname,
			},
			globals: {
				...globals.browser,
			},
		},
		plugins: {
			import: importPlugin,
			'local-rules': localRules,
		},
		rules: {
			semi: ['warn', 'always'],
			'prettier/prettier': ['warn', { endOfLine: 'auto' }],
			'no-unused-vars': 'off', //Let '@typescript-eslint/no-unused-vars' catch the errors to allow unused function parameters (ex: in interfaces)
			'no-var': 'error',
			...importPlugin.configs.recommended.rules,
			'import/namespace': 'off',
			'import/no-unresolved': 'off',
			'import/order': ['warn', { groups: ['builtin', 'parent', 'sibling', 'index', 'external'] }],
			'import/no-self-import': 'error',
			'import/no-cycle': ['error', { maxDepth: 6, allowUnsafeDynamicCyclicDependency: true }],
			'import/no-named-as-default': 'off', // Does not work with eslint 9
			'import/no-named-as-default-member': 'off', // Does not work with eslint 9
			'local-rules/prefer-static-styles-last': 'warn',
			'local-rules/enforce-umbraco-external-imports': [
				'error',
				{
					exceptions: ['@umbraco-cms', '@open-wc/testing', '@storybook', 'msw', '.', 'vite', 'uuid', 'diff'],
				},
			],
			'local-rules/exported-string-constant-naming': [
				'error',
				{
					excludedFileNames: ['umbraco-package'],
				},
			],
			'@typescript-eslint/no-non-null-assertion': 'off',
			'@typescript-eslint/no-explicit-any': 'warn',
			'@typescript-eslint/no-unused-vars': 'error',
			'@typescript-eslint/consistent-type-exports': 'error',
			'@typescript-eslint/consistent-type-imports': 'error',
			'@typescript-eslint/no-import-type-side-effects': 'warn',
			'@typescript-eslint/no-deprecated': 'warn',
		},
	},

	// Pattern-specific overrides
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
