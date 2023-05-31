import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Slider',
	name: 'Slider Property Editor UI',
	loader: () => import('./property-editor-ui-slider.element.js'),
	meta: {
		label: 'Slider',
		propertyEditorModel: 'Umbraco.Slider',
		icon: 'umb:navigation-horizontal',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'enableRange',
					label: 'Enable range',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'initVal1',
					label: 'Initial value',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'initVal2',
					label: 'Initial value 2',
					description: 'Used when range is enabled',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'step',
					label: 'Step increments',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
			],
			defaultData: [
				{
					alias: 'initVal1',
					value: 0,
				},
				{
					alias: 'initVal2',
					value: 0,
				},
				{
					alias: 'step',
					value: 0,
				},
			],
		},
	},
};
