import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CodeEditor',
	name: 'Code Editor Property Editor UI',
	element: () => import('./property-editor-ui-code-editor.element.js'),
	meta: {
		label: 'Code Editor',
		propertyEditorSchemaAlias: 'Umbraco.CodeEditor',
		icon: 'icon-code',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'language',
					label: 'Language',
					description: 'Select the programming language.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
					config: [
						{
							alias: 'items',
							value: {
								0: { sortOrder: 1, value: 'Razor' },
								1: { sortOrder: 2, value: 'HTML' },
								2: { sortOrder: 3, value: 'CSS' },
								3: { sortOrder: 3, value: 'JavaScript' },
								4: { sortOrder: 3, value: 'TypeScript' },
								5: { sortOrder: 3, value: 'Markdown' },
								6: { sortOrder: 3, value: 'JSON' },
							},
						},
					],
				},
			],
		},
	},
};
