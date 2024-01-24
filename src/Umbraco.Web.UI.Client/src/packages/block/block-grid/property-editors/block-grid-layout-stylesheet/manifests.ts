import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGridLayoutStylesheet',
	name: 'Block Grid Layout Stylesheet Property Editor UI',
	js: () => import('./property-editor-ui-block-grid-layout-stylesheet.element.js'),
	meta: {
		label: 'Block Grid Layout Stylesheet',
		icon: 'icon-document',
		group: 'common',
	},
};
