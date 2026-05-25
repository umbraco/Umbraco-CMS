import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbSectionSidebarMenuWithEntityActionsElement from './section-sidebar-menu-with-entity-actions.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.SectionSidebarAppMenuWithEntityActions',
		matchKind: 'menuWithEntityActions',
		matchType: 'sectionSidebarApp',
		manifest: {
			type: 'sectionSidebarApp',
			element: UmbSectionSidebarMenuWithEntityActionsElement,
		},
	},
];
