import { manifests as hasValueManifests } from './has-value/manifests.js';
import { manifests as writableManifests } from './writable/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...hasValueManifests, ...writableManifests];
