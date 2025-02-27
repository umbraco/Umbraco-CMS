  /** @type {import('eslint').Rule.RuleModule} */
module.exports = {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ensures the use of the `import type` operator from the `src/core/models/index.ts` file.',
      category: 'Best Practices',
      recommended: true,
    },
    fixable: 'code',
    schema: [],
  },
  create: function (context) {
    return {
      ImportDeclaration: function (node) {
        if (
          node.source.parent.importKind !== 'type' &&
          (node.source.value.endsWith('/models') || node.source.value === 'router-slot/model')
        ) {
          const sourceCode = context.getSourceCode();
          const nodeSource = sourceCode.getText(node);
          context.report({
            node,
            message: 'Use `import type` instead of `import`.',
            fix: (fixer) => fixer.replaceText(node, nodeSource.replace('import', 'import type')),
          });
        }
      },
    };
  },
};