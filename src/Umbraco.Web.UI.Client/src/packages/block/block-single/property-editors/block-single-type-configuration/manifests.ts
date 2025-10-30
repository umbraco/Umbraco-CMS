import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockSingleTypeConfiguration',
	name: 'Block Single Type Configuration Property Editor UI',
	element: () => import('./property-editor-ui-block-single-type-configuration.element.js'),
	meta: {
		label: 'Block Single Type Configuration',
		icon: 'icon-autofill',
		group: 'common',
	},
};
