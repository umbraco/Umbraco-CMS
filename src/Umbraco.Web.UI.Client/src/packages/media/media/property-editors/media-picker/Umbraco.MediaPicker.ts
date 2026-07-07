import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Media Picker',
	alias: 'Umbraco.MediaPicker3',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaPicker',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaTypePicker',
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
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaEntityPicker',
					config: [{ alias: 'validationLimit', value: { min: 0, max: 1 } }],
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
				{
					alias: 'altTextMode',
					label: 'Alternative text',
					description:
						"Controls alternative text for picked media. 'Off' is for non-image content (files, PDFs) where alt text is not applicable. 'Show alt text field' prompts editors to describe the image. 'Mark as decorative' indicates the image adds no information — a confirmation message is shown and no alt text is required.",
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
					config: [
						{
							alias: 'items',
							value: [
								{ name: 'Off (non-image content)', value: 'off' },
								{ name: 'Show alt text field', value: 'altText' },
								{ name: 'Mark as decorative image', value: 'decorative' },
							],
						},
					],
				},
				{
					alias: 'hideZoomCrop',
					label: 'Hide crop zoom control',
					description: 'Remove the zoom slider from the crop editor.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'enableAltTextPerCrop',
					label: 'Alternative text per crop',
					description:
						"Enable individual alt text per image crop. Only applies when 'Alternative text' is set to 'Show alt text field'. Only useful when crops represent entirely different images.",
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
			defaultData: [{ alias: 'altTextMode', value: 'altText' }],
		},
	},
};
