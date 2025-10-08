export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.MenuItem.Collapse',
		name: 'Collapse Document Menu Item',
		api: () => import('./collapse-menu-item.entity-action.js'),
		forEntityTypes: ['document-type-root', 'media-type-root', 'member-type-root', 'data-type-root'],
		weight: -10,
		meta: {
			label: 'Collapse Menu Item',
			icon: 'icon-wand',
		},
	},
];
