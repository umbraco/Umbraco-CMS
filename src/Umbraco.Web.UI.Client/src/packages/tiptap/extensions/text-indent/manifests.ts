export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TextIndent',
		name: 'Text Indent Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapTextIndentExtensionApi })),
		meta: {
			icon: 'icon-indent',
			label: 'Text Indent',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextIndent',
		name: 'Text Indent Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarTextIndentExtensionApi })),
		forExtensions: ['Umb.Tiptap.TextIndent'],
		meta: {
			alias: 'indent',
			icon: 'icon-indent',
			label: 'Indent',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextOutdent',
		name: 'Text Outdent Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarTextOutdentExtensionApi })),
		forExtensions: ['Umb.Tiptap.TextIndent'],
		meta: {
			alias: 'outdent',
			icon: 'icon-outdent',
			label: 'Outdent',
		},
	},
];
