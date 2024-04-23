import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MultipleTextString',
	name: 'Multiple Text String Property Editor UI',
	element: () => import('./property-editor-ui-multiple-text-string.element.js'),
	meta: {
		label: 'Multiple Text String',
		propertyEditorSchemaAlias: 'Umbraco.MultipleTextstring',
		icon: 'icon-ordered-list',
		group: 'lists',
		supportsReadOnly: true,
	},
};
