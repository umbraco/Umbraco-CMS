export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Strike',
		name: 'Strike Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapStrikeExtensionApi })),
		meta: {
			icon: 'icon-strikethrough',
			label: 'Strike',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Strike',
		name: 'Strike Tiptap Toolbar Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarStrikeExtensionApi })),
		forExtensions: ['Umb.Tiptap.Strike'],
		meta: {
			alias: 'strike',
			icon: 'icon-strikethrough',
			label: 'Strike',
		},
	},
];
