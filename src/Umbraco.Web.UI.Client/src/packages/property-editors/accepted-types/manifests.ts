import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.AcceptedUploadTypes',
	name: 'Accepted Upload Types Property Editor UI',
	element: () => import('./property-editor-ui-accepted-upload-types.element.js'),
	meta: {
		label: 'Accepted Upload Types',
		propertyEditorSchemaAlias: 'Umbraco.MultipleTextstring',
		icon: 'icon-ordered-list',
		group: 'lists',
		supportsReadOnly: true,
	},
};
