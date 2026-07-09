export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Undo',
		name: 'Undo Tiptap Toolbar Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarUndoExtensionApi })),
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
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarRedoExtensionApi })),
		meta: {
			alias: 'redo',
			icon: 'icon-redo',
			label: '#buttons_redo',
		},
	},
];
