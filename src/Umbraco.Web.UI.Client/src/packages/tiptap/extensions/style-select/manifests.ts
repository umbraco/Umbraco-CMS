export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'menu',
		alias: 'Umb.Tiptap.Toolbar.StyleSelect',
		name: 'Style Select Tiptap Extension',
		api: () => import('./style-select.tiptap-toolbar-api.js'),
		meta: {
			alias: 'umbStyleSelect',
			icon: 'icon-palette',
			label: 'Style Select',
			items: [
				{
					label: 'Headers',
					items: [
						{ label: 'Page header', data: 'h2', style: 'font-size: x-large;font-weight: bold;' },
						{ label: 'Section header', data: 'h3', style: 'font-size: large;font-weight: bold;' },
						{ label: 'Paragraph header', data: 'h4', style: 'font-weight: bold;' },
					],
				},
				{
					label: 'Blocks',
					items: [{ label: 'Paragraph', data: 'p' }],
				},
				{
					label: 'Containers',
					items: [
						{ label: 'Quote', data: 'blockquote', style: 'font-style: italic;' },
						{ label: 'Code', data: 'codeBlock', style: 'font-family: monospace;' },
					],
				},
			],
		},
	},
];
