import type { ManifestDashboard, ManifestSection, ManifestSidebarMenu } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Members';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Members Section',
	weight: 400,
	meta: {
		label: 'Members',
		pathname: 'members',
	},
};

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Members',
		name: 'Members Dashboard',
		weight: 10,
		loader: () => import('./dashboards/welcome/dashboard-members-welcome.element'),
		meta: {
			label: 'Members',
			sections: [sectionAlias],
			pathname: 'members',
		},
	},
];

const sidebarMenu: ManifestSidebarMenu = {
	type: 'sidebarMenu',
	alias: 'Umb.SidebarMenu.Members',
	name: 'Members Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Members',
		sections: [sectionAlias],
	},
};

export const manifests = [section, sidebarMenu, ...dashboards];
