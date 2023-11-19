module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Ensure all exported consts are prefixed with UMB_',
		},
	},
	create: function (context) {
		return {
			ExportNamedDeclaration(node) {
				if (node.declaration && node.declaration.type === 'VariableDeclaration') {
					const declaration = node.declaration.declarations[0];
					const { id, init } = declaration;

					if (id && id.type === 'Identifier' && init && init.type === 'Literal' && typeof init.value === 'string') {
						if (!id.name.startsWith('UMB_')) {
							context.report({
								node: id,
								message: 'Exported constant should be prefixed with UMB_',
							});
						}
					}
				}
			},
		};
	},
};
