import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_SETTINGS_SECTION_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: 'Umb.Menu.Templating',
		name: 'Templating Menu',
	},
	{
		type: 'sectionSidebarApp',
		kind: 'menu',
		alias: 'Umb.SectionSidebarMenu.Templating',
		name: 'Templating Section Sidebar Menu',
		weight: 200,
		meta: {
			label: '#treeHeaders_templatingGroup',
			menu: 'Umb.Menu.Templating',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
