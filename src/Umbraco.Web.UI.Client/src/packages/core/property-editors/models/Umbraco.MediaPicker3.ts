import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Media Picker 3',
	alias: 'Umbraco.MediaPicker3',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.MediaPicker3',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUI: 'Umb.PropertyEditorUI.TreePicker',
				},
				{
					alias: 'multiple',
					label: 'Pick multiple items',
					description: 'Outputs a IEnumerable',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of medias',
					propertyEditorUI: 'Umb.PropertyEditorUI.NumberRange',
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					propertyEditorUI: 'Umb.PropertyEditorUI.MediaPicker',
				},
				{
					alias: 'enableLocalFocalPoint',
					label: 'Enable Focal Point',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'crops',
					label: 'Image Crops',
					description: 'Local crops, stored on document',
					propertyEditorUI: 'Umb.PropertyEditorUI.ImageCropsConfiguration',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};
