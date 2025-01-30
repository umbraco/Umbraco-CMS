import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.AcceptedTypes',
	name: 'Accepted Types Property Editor UI',
	element: () => import('./property-editor-ui-accepted-types.element.js'),
	meta: {
		label: 'Accepted Upload Types',
		propertyEditorSchemaAlias: 'Umbraco.MultipleTextstring',
		icon: 'icon-ordered-list',
		group: 'lists',
		supportsReadOnly: true,
	},
};
