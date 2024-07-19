import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'sectionRoute',
		alias: 'Umb.SectionRoute.Workspace',
		name: 'Workspace Section Route',
		element: () => import('../workspace.element.js'),
		api: () => import('./workspace-section-route.route-entry.js'),
	},
];
