export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'styleMenu',
		alias: 'Umb.Tiptap.Toolbar.StyleSelect',
		name: 'Style Select Tiptap Extension',
		api: () => import('./style-select.tiptap-toolbar-api.js'),
		items: [
			{
				label: 'Headers',
				items: [
					{
						label: 'Page header',
						appearance: { icon: 'icon-heading-2', style: 'font-size: x-large;font-weight: bold;' },
						data: { tag: 'h2' },
					},
					{
						label: 'Section header',
						appearance: { icon: 'icon-heading-3', style: 'font-size: large;font-weight: bold;' },
						data: { tag: 'h3' },
					},
					{
						label: 'Paragraph header',
						appearance: { icon: 'icon-heading-4', style: 'font-weight: bold;' },
						data: { tag: 'h4' },
					},
				],
			},
			{
				label: 'Blocks',
				items: [{ label: 'Paragraph', appearance: { icon: 'icon-paragraph' }, data: { tag: 'p' } }],
			},
			{
				label: 'Containers',
				items: [
					{
						label: 'Block quote',
						appearance: { icon: 'icon-blockquote', style: 'font-style: italic;' },
						data: { tag: 'blockquote' },
					},
					{
						label: 'Code block',
						appearance: { icon: 'icon-code', style: 'font-family: monospace;' },
						data: { tag: 'codeBlock' },
					},
				],
			},
		],
		meta: {
			alias: 'umbStyleSelect',
			icon: 'icon-palette',
			label: 'Style Select',
		},
	},
];
