const forBlockEditor = 'block-grid';
const forBlockEditorWithRte = ['block-grid', 'block-rte'];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.Layout',
		name: 'Example Layout Block View',
		element: () => import('./layout-block-view.element.js'),
		forContentTypeAlias: ['oneColumnLayout', 'twoColumnLayout'],
		forBlockEditor,
	},
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.Hero',
		name: 'Example Hero Block View',
		element: () => import('./hero-block-view.element.js'),
		forContentTypeAlias: 'heroBlock',
		forBlockEditor,
	},
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.Image',
		name: 'Example Image Block View',
		element: () => import('./image-block-view.element.js'),
		forContentTypeAlias: 'imageBlock',
		forBlockEditor,
	},
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.Text',
		name: 'Example Text Block View',
		element: () => import('./text-block-view.element.js'),
		forContentTypeAlias: 'textBlock',
		forBlockEditor,
	},
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.ProductTeaser',
		name: 'Example Product Teaser Block View',
		element: () => import('./product-teaser-block-view.element.js'),
		forContentTypeAlias: 'productTeaserBlock',
		forBlockEditor: forBlockEditorWithRte,
	},
	{
		type: 'blockEditorCustomView',
		alias: 'Umb.BlockEditorCustomView.Example.ArticleTeaser',
		name: 'Example Article Teaser Block View',
		element: () => import('./article-teaser-block-view.element.js'),
		forContentTypeAlias: 'articleTeaserBlock',
		forBlockEditor: forBlockEditorWithRte,
	},
];
