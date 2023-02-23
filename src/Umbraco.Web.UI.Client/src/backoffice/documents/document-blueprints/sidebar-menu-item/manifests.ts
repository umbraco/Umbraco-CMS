import type { ManifestSidebarMenuItem } from '@umbraco-cms/models';

const sidebarMenuItem: ManifestSidebarMenuItem = {
	type: 'sidebarMenuItem',
	alias: 'Umb.SidebarMenuItem.DocumentBlueprints',
	name: 'Document Blueprints Sidebar Menu Item',
	weight: 90,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sidebarMenus: ['Umb.SidebarMenu.Settings'],
		entityType: 'document-blueprint-root',
	},
};

export const manifests = [sidebarMenuItem];
