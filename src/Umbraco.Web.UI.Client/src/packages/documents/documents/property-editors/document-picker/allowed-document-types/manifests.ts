import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentPicker.AllowedDocumentTypes',
	name: 'Document Picker Allowed Document Types Property Editor UI',
	element: () =>
		import('./property-editor-ui-document-picker-allowed-document-types.element.js'),
	meta: {
		label: 'Document Picker Allowed Document Types',
		icon: 'icon-plugin',
		group: '#propertyEditorUIGroups_pickers',
	},
};