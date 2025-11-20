import { manifest as menuAliasConditionManifest } from './conditions/menu-alias.condition.js';
import { manifests as menuItemManifests } from './components/menu-item/manifests.js';
import { manifests as sectionSidebarMenuManifests } from './section-sidebar-menu/manifests.js';
import { manifests as sectionSidebarMenuWithEntityActionsManifests } from './section-sidebar-menu-with-entity-actions/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...menuItemManifests,
	...sectionSidebarMenuManifests,
	...sectionSidebarMenuWithEntityActionsManifests,
	menuAliasConditionManifest,
];
