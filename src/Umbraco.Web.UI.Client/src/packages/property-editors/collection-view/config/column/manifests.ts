import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.ColumnConfiguration',
	name: 'Collection View Column Configuration Property Editor UI',
	element: () => import('./column-configuration.element.js'),
	meta: {
		label: 'Collection View Column Configuration',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
