export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.ClearFormatting',
		name: 'Clear Formatting Tiptap Toolbar Extension',
		api: () => import('./clear-formatting.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.HtmlAttributeClass', 'Umb.Tiptap.HtmlAttributeStyle'],
		meta: {
			alias: 'clear-formatting',
			icon: 'icon-clear-formatting',
			label: 'Clear Formatting',
		},
	},
];
