export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Underline',
		name: 'Underline Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapUnderlineExtensionApi })),
		meta: {
			icon: 'icon-underline',
			label: 'Underline',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Underline',
		name: 'Underline Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarUnderlineExtensionApi })),
		forExtensions: ['Umb.Tiptap.Underline'],
		meta: {
			alias: 'underline',
			icon: 'icon-underline',
			label: 'Underline',
		},
	},
];
