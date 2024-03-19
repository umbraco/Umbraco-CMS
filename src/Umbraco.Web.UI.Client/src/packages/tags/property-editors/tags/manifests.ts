import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Tags',
	name: 'Tags Property Editor UI',
	js: () => import('./property-editor-ui-tags.element.js'),
	meta: {
		label: 'Tags',
		propertyEditorSchemaAlias: 'Umbraco.Tags',
		icon: 'icon-tags',
		group: 'common',
	},
};

export const manifests = [manifest];
