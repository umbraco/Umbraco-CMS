import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TimeZonePicker',
	name: 'Time Zone Picker Property Editor UI',
	element: () => import('./property-editor-ui-time-zone-picker.element.js'),
	meta: {
		label: 'Time Zone Picker',
		icon: 'icon-globe',
		group: '',
	},
};
