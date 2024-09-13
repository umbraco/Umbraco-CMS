import { manifests as menuItemManifests } from './components/menu-item/manifests.js';
import { manifest as menuAliasConditionManifest } from './conditions/menu-alias.condition.js';

export const manifests: Array<UmbExtensionManifest> = [...menuItemManifests, menuAliasConditionManifest];
