/** @type {import('eslint').Rule.RuleModule}*/
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Ensure all exported string constants should be in uppercase with words separated by underscores and prefixed with UMB_',
		},
		schema: [
			{
				type: 'object',
				properties: {
					excludedFileNames: {
						type: 'array',
						items: {
							type: 'string',
						},
					},
				},
				additionalProperties: false,
			},
		],
	},
	create: function (context) {
		const excludedFileNames = context.options[0]?.excludedFileNames || [];
		return {
			ExportNamedDeclaration(node) {
				const fileName = context.filename;

				if (excludedFileNames.some((excludedFileName) => fileName.includes(excludedFileName))) {
					// Skip the rule check for files in the excluded list
					return;
				}

				if (node.declaration && node.declaration.type === 'VariableDeclaration') {
					const declaration = node.declaration.declarations[0];
					const { id, init } = declaration;

					if (id && id.type === 'Identifier' && init && init.type === 'Literal' && typeof init.value === 'string') {
						const isValidName = /^[A-Z]+(_[A-Z]+)*$/.test(id.name);

						if (!isValidName || !id.name.startsWith('UMB_')) {
							context.report({
								node: id,
								message:
									'Exported string constant should be in uppercase with words separated by underscores and prefixed with UMB_',
							});
						}
					}
				}
			},
		};
	},
};
