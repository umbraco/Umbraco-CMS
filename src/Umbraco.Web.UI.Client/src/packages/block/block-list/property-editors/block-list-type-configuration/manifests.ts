import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockListTypeConfiguration',
	name: 'Block List Type Configuration Property Editor UI',
	element: () => import('./property-editor-ui-block-list-type-configuration.element.js'),
	meta: {
		label: 'Block List Type Configuration',
		icon: 'icon-autofill',
		group: 'common',
	},
};
