import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Label',
	name: 'Label Property Editor UI',
	element: () => import('./property-editor-ui-label.element.js'),
	meta: {
		label: 'Label',
		icon: 'icon-readonly',
		group: 'pickers',
		propertyEditorSchemaAlias: 'Umbraco.Label',
	},
};
