export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Undo',
		name: 'Undo Tiptap Toolbar Extension',
		api: () => import('./undo.tiptap-toolbar-api.js'),
		meta: {
			alias: 'undo',
			icon: 'icon-undo',
			label: '#buttons_undo',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Redo',
		name: 'Redo Tiptap Toolbar Extension',
		api: () => import('./redo.tiptap-toolbar-api.js'),
		meta: {
			alias: 'redo',
			icon: 'icon-redo',
			label: '#buttons_redo',
		},
	},
];
