import { DOCUMENT_ROOT_ENTITY_TYPE } from './documents';
import type {
	ManifestSectionSidebarAppMenuWithEntityActionsKind,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

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

const menuSectionSidebarApp: ManifestSectionSidebarAppMenuWithEntityActionsKind = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: 'Umb.SidebarMenu.Content',
	name: 'Content Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Content',
		menu: 'Umb.Menu.Content',
		entityType: DOCUMENT_ROOT_ENTITY_TYPE,
	},
	conditions: {
		sections: [sectionAlias],
	},
};

export const manifests = [section, menuSectionSidebarApp];
