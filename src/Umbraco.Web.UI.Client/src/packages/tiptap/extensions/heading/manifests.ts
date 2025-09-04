export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Heading',
		name: 'Headings Tiptap Extension',
		api: () => import('./heading.tiptap-api.js'),
		meta: {
			icon: 'icon-heading-1',
			label: 'Headings',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading1',
		name: 'Heading 1 Tiptap Toolbar Extension',
		api: () => import('./heading1.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading1',
			icon: 'icon-heading-1',
			label: 'Heading 1',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading2',
		name: 'Heading 2 Tiptap Toolbar Extension',
		api: () => import('./heading2.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading2',
			icon: 'icon-heading-2',
			label: 'Heading 2',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading3',
		name: 'Heading 3 Tiptap Toolbar Extension',
		api: () => import('./heading3.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading3',
			icon: 'icon-heading-3',
			label: 'Heading 3',
		},
	},
];
