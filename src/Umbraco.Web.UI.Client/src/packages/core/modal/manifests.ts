import { manifests as commonManifests } from './common/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...commonManifests, ...conditionManifests];
