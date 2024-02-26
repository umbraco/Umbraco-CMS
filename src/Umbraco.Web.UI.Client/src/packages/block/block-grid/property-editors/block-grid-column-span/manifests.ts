import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.BlockGridColumnSpan',
	name: 'Block Grid Column Span Property Editor UI',
	js: () => import('./property-editor-ui-block-grid-column-span.element.js'),
	meta: {
		label: 'Block Grid Column Span',
		icon: 'icon-document',
		group: 'common',
	},
};
