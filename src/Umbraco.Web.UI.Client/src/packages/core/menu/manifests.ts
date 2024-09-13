import { manifests as menuItemManifests } from './components/menu-item/manifests.js';
import { manifest as menuAliasConditionManifest } from './conditions/menu-alias.condition.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...menuItemManifests,
	menuAliasConditionManifest,
];
