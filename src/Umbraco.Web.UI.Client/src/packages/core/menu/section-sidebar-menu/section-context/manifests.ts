import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'sectionContext',
		alias: 'Umb.SectionContext.SectionSidebarMenu',
		name: 'Section Sidebar Menu Section Context',
		api: () => import('./section-sidebar-menu.section-context.js'),
	},
];
