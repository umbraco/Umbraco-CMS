import { UmbWorkspaceElement } from '../workspace.element.js';
import { UmbWorkspaceSectionRouteEntry } from './workspace-section-route.route-entry.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionRoute',
		alias: 'Umb.SectionRoute.Workspace',
		name: 'Workspace Section Route',
		element: UmbWorkspaceElement,
		api: UmbWorkspaceSectionRouteEntry,
	},
];
