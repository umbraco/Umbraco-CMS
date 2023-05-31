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
					propertyEditorUi: 'Umb.PropertyEditorUi.TreePicker',
				},
				{
					alias: 'multiple',
					label: 'Pick multiple items',
					description: 'Outputs a IEnumerable',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of medias',
					propertyEditorUi: 'Umb.PropertyEditorUi.NumberRange',
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					propertyEditorUi: 'Umb.PropertyEditorUi.MediaPicker',
				},
				{
					alias: 'enableLocalFocalPoint',
					label: 'Enable Focal Point',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'crops',
					label: 'Image Crops',
					description: 'Local crops, stored on document',
					propertyEditorUi: 'Umb.PropertyEditorUi.ImageCropsConfiguration',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUi: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
