export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.HorizontalRule',
		name: 'Horizontal Rule Tiptap Extension',
		api: () => import('./horizontal-rule.tiptap-api.js'),
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
		api: () => import('./horizontal-rule.tiptap-toolbar-api.js'),
		meta: {
			alias: 'horizontalRule',
			icon: 'icon-horizontal-rule',
			label: 'Horizontal Rule',
		},
	},
];
