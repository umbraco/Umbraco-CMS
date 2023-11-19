module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Ensure all exported consts are prefixed with UMB_ and follow the naming convention',
		},
	},
	create: function (context) {
		return {
			ExportNamedDeclaration(node) {
				if (node.declaration && node.declaration.type === 'VariableDeclaration') {
					const declaration = node.declaration.declarations[0];
					const { id, init } = declaration;

					if (id && id.type === 'Identifier' && init && init.type === 'Literal' && typeof init.value === 'string') {
						const isValidName = /^[A-Z]+(_[A-Z]+)*$/.test(id.name);

						if (!isValidName || !id.name.startsWith('UMB_')) {
							context.report({
								node: id,
								message:
									'Exported constant should be in uppercase with words separated by underscores and prefixed with UMB_',
							});
						}
					}
				}
			},
		};
	},
};
