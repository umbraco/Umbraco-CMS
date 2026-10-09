export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.CodeBlock',
		name: 'Code Block Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapCodeBlockExtensionApi })),
		meta: {
			icon: 'icon-code',
			label: 'Code Block',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.CodeBlock',
		name: 'Code Block Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarCodeBlockExtensionApi })),
		forExtensions: ['Umb.Tiptap.CodeBlock'],
		meta: {
			alias: 'codeBlock',
			icon: 'icon-code',
			label: 'Code Block',
		},
	},
];
