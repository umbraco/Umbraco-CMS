import type { ManifestBlockEditorCustomView } from '../block-custom-view/block-editor-custom-view.extension.js';

export const manifest: ManifestBlockEditorCustomView = {
	type: 'blockEditorCustomView',
	alias: 'Umb.blockEditorCustomView.TestView',
	name: 'Block Editor Custom View Test',
	element: () => import('./custom-view.element.js'),
	forContentTypeAlias: 'elementTypeHeadline',
	forBlockEditor: 'block-grid',
};
