export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HorizontalRule',
		name: 'Horizontal Rule Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapHorizontalRuleExtensionApi })),
		meta: {
			icon: 'icon-horizontal-rule',
			label: 'Horizontal Rule',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.HorizontalRule',
		name: 'Horizontal Rule Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarHorizontalRuleExtensionApi })),
		forExtensions: ['Umb.Tiptap.HorizontalRule'],
		meta: {
			alias: 'horizontalRule',
			icon: 'icon-horizontal-rule',
			label: 'Horizontal Rule',
		},
	},
];
