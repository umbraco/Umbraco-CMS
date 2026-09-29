export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Bold',
		name: 'Bold Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapBoldExtensionApi })),
		meta: {
			icon: 'icon-bold',
			label: 'Bold',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Bold',
		name: 'Bold Tiptap Toolbar Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarBoldExtensionApi })),
		forExtensions: ['Umb.Tiptap.Bold'],
		meta: {
			alias: 'bold',
			icon: 'icon-bold',
			label: '#buttons_bold',
		},
	},
];
