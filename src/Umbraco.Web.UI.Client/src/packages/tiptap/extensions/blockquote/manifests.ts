export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Blockquote',
		name: 'Blockquote Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapBlockquoteExtensionApi })),
		meta: {
			icon: 'icon-blockquote',
			label: 'Blockquote',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Blockquote',
		name: 'Blockquote Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarBlockquoteExtensionApi })),
		forExtensions: ['Umb.Tiptap.Blockquote'],
		meta: {
			alias: 'blockquote',
			icon: 'icon-blockquote',
			label: 'Blockquote',
		},
	},
];
