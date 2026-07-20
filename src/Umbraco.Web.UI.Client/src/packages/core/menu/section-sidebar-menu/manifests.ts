import { manifests as sectionContextManifests } from './section-context/manifests.js';
import { manifests as globalContextManifests } from './global-context/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbSectionSidebarMenuElement from './section-sidebar-menu.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.SectionSidebarAppMenu',
		matchKind: 'menu',
		matchType: 'sectionSidebarApp',
		manifest: {
			type: 'sectionSidebarApp',
			element: UmbSectionSidebarMenuElement,
		},
	},
	...sectionContextManifests,
	...globalContextManifests,
];
