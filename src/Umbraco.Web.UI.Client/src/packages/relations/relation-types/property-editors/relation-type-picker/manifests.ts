import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.RelationTypePicker',
	name: 'Relation Type Picker Property Editor UI',
	element: () => import('./property-editor-ui-relation-type-picker.element.js'),
	meta: {
		label: 'Relation Type Picker',
		icon: 'icon-trafic',
		group: '#propertyEditorUIGroups_advanced',
		keywords: ['relation', 'relations', 'pick'],
		supportsReadOnly: true,
	},
};
