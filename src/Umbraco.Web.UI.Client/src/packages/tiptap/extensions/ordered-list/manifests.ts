export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.OrderedList',
		name: 'Ordered List Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapOrderedListExtensionApi })),
		meta: {
			icon: 'icon-ordered-list',
			label: 'Ordered List',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.OrderedList',
		name: 'Ordered List Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarOrderedListExtensionApi })),
		forExtensions: ['Umb.Tiptap.OrderedList'],
		meta: {
			alias: 'orderedList',
			icon: 'icon-ordered-list',
			label: 'Ordered List',
		},
	},
];
