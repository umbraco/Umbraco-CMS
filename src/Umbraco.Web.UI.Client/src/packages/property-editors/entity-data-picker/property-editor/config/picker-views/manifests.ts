import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.EntityDataPicker.PickerViewsConfiguration',
	name: 'Entity Data Picker Picker Views Configuration Property Editor UI',
	element: () => import('./picker-views-configuration.element.js'),
	meta: {
		label: 'Entity Data Picker Picker Views Configuration',
		icon: 'icon-grid',
		group: 'pickers',
	},
};
