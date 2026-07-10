import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbSectionSidebarMenuGlobalContext } from './section-sidebar-menu.global-context.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.SectionSidebarMenu',
		name: 'Section Sidebar Menu Global Context',
		api: UmbSectionSidebarMenuGlobalContext,
	},
];
