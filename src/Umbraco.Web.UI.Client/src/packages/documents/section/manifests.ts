import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import { UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
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
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_CONTENT_SECTION_ALIAS,
			},
		],
	},
	{
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
	},
];
