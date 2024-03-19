import { UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_CONTENT_MENU_ALIAS } from './documents/index.js';
import type {
	ManifestSection,
	ManifestSectionSidebarAppMenuWithEntityActionsKind,
} from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Content';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Content Section',
	weight: 600,
	meta: {
		label: 'Content',
		pathname: 'content',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionUserPermission',
			match: sectionAlias,
		},
	],
};

const menuSectionSidebarApp: ManifestSectionSidebarAppMenuWithEntityActionsKind = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: 'Umb.SidebarMenu.Content',
	name: 'Content Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Content',
		menu: UMB_CONTENT_MENU_ALIAS,
		entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: sectionAlias,
		},
	],
};

export const manifests = [section, menuSectionSidebarApp];
