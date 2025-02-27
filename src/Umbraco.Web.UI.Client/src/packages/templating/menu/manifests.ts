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
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
