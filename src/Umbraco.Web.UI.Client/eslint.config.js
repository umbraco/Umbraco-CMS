import js from '@eslint/js';
import globals from 'globals';
import importPlugin from 'eslint-plugin-import';
import localRules from 'eslint-plugin-local-rules';
import wcPlugin from 'eslint-plugin-wc';
import litPlugin from 'eslint-plugin-lit';
import litA11yPlugin from 'eslint-plugin-lit-a11y';
import storybookPlugin from 'eslint-plugin-storybook';
import eslintPluginPrettierRecommended from 'eslint-plugin-prettier/recommended';
import tseslint from 'typescript-eslint';

export default [
	// Recommended config applied to all files
	js.configs.recommended,
	...tseslint.configs.recommended,
	eslintPluginPrettierRecommended,
	wcPlugin.configs['flat/recommended'],
	litPlugin.configs['flat/recommended'],
	localRules.configs.all,

	// Global ignores
	{
		ignores: [
			'**/rollup.config.js',
			'**/vite.config.ts',
			'src/external',
			'src/packages/core/icon-registry/icons',
			'src/packages/core/icon-registry/icons.ts',
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
			'lit-a11y': litA11yPlugin,
			storybook: storybookPlugin,
		},
		rules: {
			semi: ['warn', 'always'],
			"prettier/prettier": ["warn", { "endOfLine": "auto" }],
			'no-unused-vars': 'off', //Let '@typescript-eslint/no-unused-vars' catch the errors to allow unused function parameters (ex: in interfaces)
			'no-var': 'error',
			...importPlugin.configs.recommended.rules,
			'import/namespace': 'off',
			'import/no-unresolved': 'off',
			'import/order': ['warn', { groups: ['builtin', 'parent', 'sibling', 'index', 'external'] }],
			'import/no-self-import': 'error',
			'import/no-cycle': ['error', { maxDepth: 6, allowUnsafeDynamicCyclicDependency: true }],
			'local-rules/prefer-static-styles-last': 'warn',
			'local-rules/enforce-umbraco-external-imports': [
				'error',
				{
					exceptions: ['@umbraco-cms', '@open-wc/testing', '@storybook', 'msw', '.', 'vite'],
				},
			],
			'local-rules/exported-string-constant-naming': [
				'error',
				{
					excludedFileNames: ['umbraco-package', 'input-tiny-mce.defaults'], // TODO: what to do about the tiny mce defaults?
				},
			],
			'@typescript-eslint/no-non-null-assertion': 'off',
			'@typescript-eslint/no-explicit-any': 'warn',
			'@typescript-eslint/no-unused-vars': 'error',
			'@typescript-eslint/consistent-type-exports': 'error',
			'@typescript-eslint/consistent-type-imports': 'error',
			'@typescript-eslint/no-import-type-side-effects': 'warn',
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
