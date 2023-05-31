import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Media Picker 3',
	alias: 'Umbraco.MediaPicker3',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaPicker3',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TreePicker',
				},
				{
					alias: 'multiple',
					label: 'Pick multiple items',
					description: 'Outputs a IEnumerable',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of medias',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaPicker',
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
