import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CodeEditor',
	name: 'Code Editor Property Editor UI',
	element: () => import('./property-editor-ui-code-editor.element.js'),
	meta: {
		label: 'Code Editor',
		propertyEditorSchemaAlias: 'Umbraco.Plain.String',
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
								{ name: 'C#', value: 'csharp' },
								{ name: 'CSS', value: 'css' },
								{ name: 'HTML', value: 'html' },
								{ name: 'JavaScript', value: 'javascript' },
								{ name: 'JSON', value: 'json' },
								{ name: 'Markdown', value: 'markdown' },
								{ name: 'Razor (CSHTML)', value: 'razor' },
								{ name: 'TypeScript', value: 'typescript' },
							],
						},
					],
				},
				{
					alias: 'height',
					label: '#codeEditor_heightConfigLabel',
					description: '{#codeEditor_heightConfigDescription}',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [{ alias: 'min', value: 0 }],
				},
			],
			defaultData: [
				{ alias: 'language', value: 'javascript' },
				{ alias: 'height', value: 400 },
			],
		},
	},
};
