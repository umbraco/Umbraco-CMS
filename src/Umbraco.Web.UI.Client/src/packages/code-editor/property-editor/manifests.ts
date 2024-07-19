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
							value: [
								{ name: 'C#', value: 'csharp' },
								{ name: 'Razor', value: 'csharp' },
								{ name: 'HTML', value: 'html' },
								{ name: 'CSS', value: 'css' },
								{ name: 'JavaScript', value: 'javascript' },
								{ name: 'TypeScript', value: 'typescript' },
								{ name: 'Markdown', value: 'markdown' },
								{ name: 'JSON', value: 'json' },
							],
						},
					],
				},
			],
		},
	},
};
