import { manifests as entityTypeConditionManifests } from './entity-type/manifests.js';
import { manifests as uniqueConditionManifests } from './unique/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...entityTypeConditionManifests, ...uniqueConditionManifests];
