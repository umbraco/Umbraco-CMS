import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
	name: 'Content Picker Source Type Property Editor UI',
	element: () => import('./property-editor-ui-content-picker-source-type.element.js'),
	meta: {
		label: 'Content Picker Source Type Picker',
		icon: 'icon-page-add',
		group: 'pickers',
	},
};
