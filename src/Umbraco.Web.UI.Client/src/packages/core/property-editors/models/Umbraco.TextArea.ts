import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Textarea',
	alias: 'Umbraco.TextArea',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TextArea',
		settings: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty - no character limit',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
			],
		},
	},
};
