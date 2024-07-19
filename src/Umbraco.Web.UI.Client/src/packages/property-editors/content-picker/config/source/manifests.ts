import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ContentPicker.Source',
	name: 'Content Picker Source Property Editor UI',
	element: () => import('./property-editor-ui-content-picker-source.element.js'),
	meta: {
		label: 'Content Picker Source',
		icon: 'icon-page-add',
		group: 'pickers',
	},
};
