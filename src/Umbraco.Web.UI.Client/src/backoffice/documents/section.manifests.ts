import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { ManifestSectionSidebarAppMenuKind } from '@umbraco-cms/backoffice/extension-registry';

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

const menuSectionSidebarApp: ManifestSectionSidebarAppMenuKind = {
	type: 'sectionSidebarApp',
	kind: 'menu',
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

export const manifests = [section, menuSectionSidebarApp];
