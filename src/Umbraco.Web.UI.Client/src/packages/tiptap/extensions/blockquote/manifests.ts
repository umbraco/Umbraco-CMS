export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Blockquote',
		name: 'Blockquote Tiptap Extension',
		api: () => import('./blockquote.tiptap-api.js'),
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
		api: () => import('./blockquote.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Blockquote'],
		meta: {
			alias: 'blockquote',
			icon: 'icon-blockquote',
			label: 'Blockquote',
		},
	},
];
