import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CodeEditor',
	name: 'Code Editor Property Editor UI',
	element: () => import('./property-editor-ui-code-editor.element.js'),
	meta: {
		label: 'Code Editor',
		propertyEditorSchemaAlias: 'Umbraco.Plain.Text',
		icon: 'icon-code',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'language',
					label: '#codeEditor_languageConfigLabel',
					description: '{#codeEditor_languageConfigDescription}',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
					config: [
						{
							alias: 'items',
							value: [
								{ name: 'JavaScript', value: 'javascript', selected: true },
								{ name: 'HTML', value: 'html' },
								{ name: 'TypeScript', value: 'typescript' },
								{ name: 'C#', value: 'csharp' },
								{ name: 'Razor', value: 'csharp' },
								{ name: 'CSS', value: 'css' },
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
