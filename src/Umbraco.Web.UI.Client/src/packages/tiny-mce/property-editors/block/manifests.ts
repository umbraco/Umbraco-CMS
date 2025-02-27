import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockRteTypeConfiguration',
	name: 'Block Rte Type Configuration Property Editor UI',
	js: () => import('./property-editor-ui-block-rte-type-configuration.element.js'),
	meta: {
		label: 'Block Rte Type Configuration',
		icon: 'icon-autofill',
		group: 'common',
	},
};
