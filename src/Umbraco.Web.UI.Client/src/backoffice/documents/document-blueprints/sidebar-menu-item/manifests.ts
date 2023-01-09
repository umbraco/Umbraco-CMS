import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.DocumentBlueprints',
	name: 'Document Blueprints Sidebar Menu Item',
	weight: 400,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sections: ['Umb.Section.Settings'],
		entityType: 'document-blueprint-root',
	},
};

export const manifests = [sidebarMenuItem];
