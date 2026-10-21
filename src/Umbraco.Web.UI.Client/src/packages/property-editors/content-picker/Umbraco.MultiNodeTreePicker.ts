import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Multi Node Tree Picker',
	alias: 'Umbraco.MultiNodeTreePicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker',
		settings: {
			properties: [
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set the minimum and maximum number of items allowed.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore user start nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'startNode',
					label: 'Node type',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.Source',
				},
			],
		},
	},
};
