import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Multi Node Tree Picker',
	alias: 'Umbraco.MultiNodeTreePicker',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.MultiNodeTreePicker',
		settings: {
			properties: [
				{
					alias: 'minNumber',
					label: 'Minimum number of items',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'maxNumber',
					label: 'Maximum number of items',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore user start nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
			defaultData: [
				{
					alias: 'minNumber',
					value: 0,
				},
				{
					alias: 'maxNumber',
					value: 0,
				},
			],
		},
	},
};
