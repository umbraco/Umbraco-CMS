import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Textarea',
	alias: 'Umbraco.TextArea',
	meta: {
		config: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty - no character limit',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
			],
		},
	},
};
