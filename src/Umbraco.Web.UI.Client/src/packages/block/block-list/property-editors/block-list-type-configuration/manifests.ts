import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockListTypeConfiguration',
	name: 'Block List Type Configuration Property Editor UI',
	js: () => import('./property-editor-ui-block-list-type-configuration.element.js'),
	meta: {
		label: 'Block List Type Configuration',
		propertyEditorSchemaAlias: '',
		icon: 'icon-autofill',
		group: 'common',
	},
};
