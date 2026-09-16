import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import litPlugin from 'eslint-plugin-lit';
import wcPlugin from 'eslint-plugin-wc';
import prettierConfig from 'eslint-config-prettier';

export default tseslint.config(
  // Base recommended configs
  js.configs.recommended,
  ...tseslint.configs.recommendedTypeChecked,

  // Global ignores
  {
    ignores: ['node_modules/**', 'dist/**', '**/*.js', '**/*.d.ts', 'src/api/**', 'vite.config.ts'],
  },

  // TypeScript configuration
  {
    files: ['**/*.ts'],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      // TypeScript-specific rules
      '@typescript-eslint/no-explicit-any': 'warn',
      '@typescript-eslint/no-unused-vars': [
        'error',
        {
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
        },
      ],
      '@typescript-eslint/explicit-module-boundary-types': 'off',
      '@typescript-eslint/no-non-null-assertion': 'off',
      '@typescript-eslint/no-unsafe-assignment': 'off',
      '@typescript-eslint/no-unsafe-member-access': 'off',

      // Prefer optional chaining
      '@typescript-eslint/prefer-optional-chain': 'error',
      '@typescript-eslint/prefer-nullish-coalescing': 'error',

      // Code style
      quotes: ['error', 'single', { avoidEscape: true, allowTemplateLiterals: true }],
    },
  },

  // Lit Web Components configuration
  {
    files: ['**/*.modal.ts'],
    plugins: {
      lit: litPlugin,
      wc: wcPlugin,
    },
    rules: {
      ...litPlugin.configs.recommended.rules,
      ...wcPlugin.configs.recommended.rules,

      // Lit-specific rules
      'lit/no-invalid-html': 'error',
      'lit/no-useless-template-literals': 'error',
      'lit/attribute-value-entities': 'error',
      'lit/binding-positions': 'error',
      'lit/no-duplicate-template-bindings': 'error',
      'lit/no-property-change-update': 'error',

      // Web Components best practices
      'wc/no-self-class': 'error',
      'wc/require-listener-teardown': 'error',
      'wc/guard-super-call': 'off',

      // Disable unbound-method for Lit elements
      '@typescript-eslint/unbound-method': 'off',
    },
  },

  // Prettier integration (must be last to disable conflicting rules)
  prettierConfig,
);
