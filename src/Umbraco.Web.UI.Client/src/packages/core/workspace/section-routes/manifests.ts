export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionRoute',
		alias: 'Umb.SectionRoute.Workspace',
		name: 'Workspace Section Route',
		element: () => import('../workspace.element.js'),
		api: () => import('./workspace-section-route.route-entry.js'),
	},
];
