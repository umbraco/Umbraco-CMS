export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Italic',
		name: 'Italic Tiptap Extension',
		api: () => import('./italic.tiptap-api.js'),
		meta: {
			icon: 'icon-italic',
			label: 'Italic',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Italic',
		name: 'Italic Tiptap Toolbar Extension',
		api: () => import('./italic.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Italic'],
		meta: {
			alias: 'italic',
			icon: 'icon-italic',
			label: '#buttons_italic',
		},
	},
];
