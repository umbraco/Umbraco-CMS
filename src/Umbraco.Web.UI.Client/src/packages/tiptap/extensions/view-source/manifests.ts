export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.SourceEditor',
		name: 'Source Editor Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarSourceEditorExtensionApi })),
		meta: {
			alias: 'umbSourceEditor',
			icon: 'icon-code-xml',
			label: '#general_viewSourceCode',
		},
	},
];
