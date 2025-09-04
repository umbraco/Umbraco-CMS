export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.OrderedList',
		name: 'Ordered List Tiptap Extension',
		api: () => import('./ordered-list.tiptap-api.js'),
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
		api: () => import('./ordered-list.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.OrderedList'],
		meta: {
			alias: 'orderedList',
			icon: 'icon-ordered-list',
			label: 'Ordered List',
		},
	},
];
