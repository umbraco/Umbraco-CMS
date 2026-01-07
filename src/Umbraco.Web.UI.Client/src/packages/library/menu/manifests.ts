import { UMB_LIBRARY_SECTION_ALIAS } from '../section/index.js';
import { UMB_LIBRARY_MENU_ALIAS } from './constants.js';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import type { ManifestMenu, ManifestSectionSidebarAppMenuKind } from '@umbraco-cms/backoffice/menu';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_LIBRARY_MENU_ALIAS,
	name: 'Library Menu',
};

const sectionSidebarApp: ManifestSectionSidebarAppMenuKind = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SidebarMenu.Library',
	name: 'Library Sidebar Menu',
	weight: 100,
	meta: {
		label: '#sections_library',
		menu: UMB_LIBRARY_MENU_ALIAS,
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: UMB_LIBRARY_SECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [menu, sectionSidebarApp];
