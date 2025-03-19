import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Multi URL Picker',
	alias: 'Umbraco.MultiUrlPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MultiUrlPicker',
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
			],
			defaultData: [
				{ alias: 'minNumber', value: 0 },
				{ alias: 'maxNumber', value: 0 },
			],
		},
	},
};
