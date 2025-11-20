export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'menu',
		alias: 'Umb.Tiptap.Toolbar.FontFamily',
		name: 'Font Family Tiptap Toolbar Extension',
		api: () => import('./font-family.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.HtmlAttributeStyle', 'Umb.Tiptap.HtmlTagSpan'],
		items: [
			{ label: 'Sans serif', appearance: { style: 'font-family: sans-serif;' }, data: 'sans-serif' },
			{ label: 'Serif', appearance: { style: 'font-family: serif;' }, data: 'serif' },
			{ label: 'Monospace', appearance: { style: 'font-family: monospace;' }, data: 'monospace' },
			{ label: 'Cursive', appearance: { style: 'font-family: cursive;' }, data: 'cursive' },
			{ label: 'Fantasy', appearance: { style: 'font-family: fantasy;' }, data: 'fantasy' },
		],
		meta: {
			alias: 'umbFontFamily',
			icon: 'icon-ruler-alt',
			label: 'Font family',
		},
	},
];
