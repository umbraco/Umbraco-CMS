export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.CodeBlock',
		name: 'Code Block Tiptap Extension',
		api: () => import('./code-block.tiptap-api.js'),
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
		api: () => import('./code-block.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.CodeBlock'],
		meta: {
			alias: 'codeBlock',
			icon: 'icon-code',
			label: 'Code Block',
		},
	},
];
