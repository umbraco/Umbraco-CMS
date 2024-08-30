import type { ManifestBlockEditorCustomView } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestBlockEditorCustomView> = [
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.blockEditorCustomView.TestView',
		name: 'Block Editor Custom View Test',
		element: () => import('./block-custom-view.js'),
		forContentTypeAlias: 'headlineUmbracoDemoBlock',
		forBlockEditor: 'block-list',
	},
];
