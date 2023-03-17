import type { ManifestKind, ManifestTypes } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Content';

const section: ManifestTypes = {
	type: 'section',
	alias: sectionAlias,
	name: 'Content Section',
	weight: 600,
	meta: {
		label: 'Content',
		pathname: 'content',
	},
};

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menuSectionSidebarApp',
	alias: 'Umb.SidebarMenu.Content',
	name: 'Content Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Content',
		menu: 'Umb.Menu.Content',
	},
	conditions: {
		sections: [sectionAlias],
	},
};

// TODO: move to a general place:
const menuSectionSidebarAppKind: ManifestKind = {
	type: 'kind',
	matchKind: 'menuSectionSidebarApp',
	matchType: 'sectionSidebarApp',
	manifest: {
		type: 'sectionSidebarApp',
		elementName: 'umb-section-sidebar-menu',
	},
};

export const manifests = [section, menuSectionSidebarApp, menuSectionSidebarAppKind];
