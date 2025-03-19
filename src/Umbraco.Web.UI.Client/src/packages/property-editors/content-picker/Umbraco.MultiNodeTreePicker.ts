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
					alias: 'minNumber',
					label: 'Minimum number of items',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [{ alias: 'min', value: 0 }],
				},
				{
					alias: 'maxNumber',
					label: 'Maximum number of items',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [{ alias: 'min', value: 0 }],
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore user access',
					description: 'allow users to choose items exceeding their access.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'startNode',
					label: 'Node type',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.Source',
				},
			],
			defaultData: [
				{ alias: 'minNumber', value: 0 },
				{ alias: 'maxNumber', value: 0 },
			],
		},
	},
};
