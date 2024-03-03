import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGridTypeConfiguration',
	name: 'Block Grid Block Configuration Property Editor UI',
	js: () => import('./property-editor-ui-block-grid-type-configuration.element.js'),
	meta: {
		label: 'Block Grid Block Configuration',
		icon: 'icon-autofill',
		group: 'blocks',
	},
};
