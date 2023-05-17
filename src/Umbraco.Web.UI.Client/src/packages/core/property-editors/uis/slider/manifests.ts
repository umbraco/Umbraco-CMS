import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.Slider',
	name: 'Slider Property Editor UI',
	loader: () => import('./property-editor-ui-slider.element'),
	meta: {
		label: 'Slider',
		propertyEditorModel: 'Umbraco.Slider',
		icon: 'umb:navigation-horizontal',
		group: 'common',
		config: {
			properties: [
				{
					alias: 'enableRange',
					label: 'Enable range',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
				{
					alias: 'initVal1',
					label: 'Initial value',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'initVal2',
					label: 'Initial value 2',
					description: 'Used when range is enabled',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'step',
					label: 'Step increments',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
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
