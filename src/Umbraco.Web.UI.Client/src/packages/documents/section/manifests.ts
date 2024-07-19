import { UMB_CONTENT_SECTION_ALIAS } from './constants.js';
import { UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';
import type {
	ManifestSection,
	ManifestSectionSidebarAppMenuWithEntityActionsKind,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const section: ManifestSection = {
	type: 'section',
	alias: UMB_CONTENT_SECTION_ALIAS,
	name: 'Content Section',
	weight: 1000,
	meta: {
		label: '#sections_content',
		pathname: 'content',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionUserPermission',
			match: UMB_CONTENT_SECTION_ALIAS,
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
		label: '#sections_content',
		menu: UMB_CONTENT_MENU_ALIAS,
		entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: UMB_CONTENT_SECTION_ALIAS,
		},
	],
};

export const manifests: Array<ManifestTypes> = [section, menuSectionSidebarApp];
