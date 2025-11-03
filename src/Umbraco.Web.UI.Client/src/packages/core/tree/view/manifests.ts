export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'treeView',
		alias: 'Umb.TreeView.Menu',
		name: 'Menu Tree View',
		element: () => import('./tree-menu-view/tree-menu-view.element.js'),
		meta: {
			label: 'Menu',
			icon: 'icon-blockquote',
		},
	},
	{
		type: 'treeView',
		alias: 'Umb.TreeView.Card',
		name: 'Card Tree View',
		element: () => import('./tree-card-view/tree-card-view.element.js'),
		meta: {
			label: 'Card',
			icon: 'icon-grid-2',
		},
	},
];
