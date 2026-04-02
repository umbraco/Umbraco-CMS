export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'menu',
		alias: 'Umb.Tiptap.Toolbar.FontSize',
		name: 'Font Size Tiptap Toolbar Extension',
		api: () => import('./font-size.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.HtmlAttributeStyle', 'Umb.Tiptap.HtmlTagSpan'],
		items: [
			{ label: '8pt', data: '8pt' },
			{ label: '10pt', data: '10pt' },
			{ label: '12pt', data: '12pt' },
			{ label: '14pt', data: '14pt' },
			{ label: '16pt', data: '16pt' },
			{ label: '18pt', data: '18pt' },
			{ label: '24pt', data: '24pt' },
			{ label: '26pt', data: '26pt' },
			{ label: '48pt', data: '48pt' },
		],
		meta: {
			alias: 'umbFontSize',
			icon: 'icon-ruler',
			label: 'Font size',
		},
	},
];
