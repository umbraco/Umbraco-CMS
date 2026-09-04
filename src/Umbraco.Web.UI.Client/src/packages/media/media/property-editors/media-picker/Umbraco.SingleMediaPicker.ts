import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Single Media Picker',
	alias: 'Umbraco.SingleMediaPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.SingleMediaPicker',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaTypePicker',
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaEntityPicker',
					config: [{ alias: 'validationLimit', value: { min: 0, max: 1 } }],
				},
				{
					alias: 'dynamicRoot',
					label: 'Dynamic root',
					description: 'Resolve the start node from the content being edited, when no start node is set',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.DynamicRoot',
				},
				{
					alias: 'enableLocalFocalPoint',
					label: 'Enable Focal Point',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'crops',
					label: 'Image Crops',
					description: 'Local crops, stored on document',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ImageCropsConfiguration',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
