export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		name: 'Example Section Sidebar Menu Playground Dashboard',
		alias: 'Example.Dashboard.SectionSidebarMenuPlayground',
		element: () => import('./section-sidebar-menu-playground.element.js'),
		weight: 3000,
		meta: {
			label: 'Section Sidebar Menu Playground',
			pathname: 'section-sidebar-menu-playground',
		},
	},
];
