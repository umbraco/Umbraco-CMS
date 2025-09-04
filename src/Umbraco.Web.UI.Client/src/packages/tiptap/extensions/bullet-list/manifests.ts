export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.BulletList',
		name: 'Bullet List Tiptap Extension',
		api: () => import('./bullet-list.tiptap-api.js'),
		meta: {
			icon: 'icon-bulleted-list',
			label: 'Bullet List',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.BulletList',
		name: 'Bullet List Tiptap Toolbar Extension',
		api: () => import('./bullet-list.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.BulletList'],
		meta: {
			alias: 'bulletList',
			icon: 'icon-bulleted-list',
			label: '#buttons_listBullet',
		},
	},
];
