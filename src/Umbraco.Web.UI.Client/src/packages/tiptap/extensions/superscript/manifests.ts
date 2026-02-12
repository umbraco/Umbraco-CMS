export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Superscript',
		name: 'Superscript Tiptap Extension',
		api: () => import('./superscript.tiptap-api.js'),
		meta: {
			icon: 'icon-superscript',
			label: 'Superscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Superscript',
		name: 'Superscript Tiptap Toolbar Extension',
		api: () => import('./superscript.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Superscript'],
		meta: {
			alias: 'superscript',
			icon: 'icon-superscript',
			label: 'Superscript',
		},
	},
];
