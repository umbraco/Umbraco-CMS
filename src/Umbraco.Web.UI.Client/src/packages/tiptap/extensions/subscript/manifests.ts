export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Subscript',
		name: 'Subscript Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapSubscriptExtensionApi })),
		meta: {
			icon: 'icon-subscript',
			label: 'Subscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Subscript',
		name: 'Subscript Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarSubscriptExtensionApi })),
		forExtensions: ['Umb.Tiptap.Subscript'],
		meta: {
			alias: 'subscript',
			icon: 'icon-subscript',
			label: 'Subscript',
		},
	},
];
