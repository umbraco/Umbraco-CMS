import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.ColumnConfiguration',
	name: 'Collection Column Configuration Property Editor UI',
	element: () => import('./column-configuration.element.js'),
	meta: {
		label: 'Collection Column Configuration',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
