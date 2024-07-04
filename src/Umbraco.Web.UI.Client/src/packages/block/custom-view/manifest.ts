import type { ManifestBlockEditorCustomView } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestBlockEditorCustomView = {
	type: 'blockEditorCustomView',
	alias: 'Umb.blockEditorCustomView.TestView',
	name: 'Block Editor Custom View Test',
	element: () => import('./custom-view.element.js'),
	forContentTypeAlias: 'elementTypeHeadline',
	forBlockEditor: 'block-grid',
};
