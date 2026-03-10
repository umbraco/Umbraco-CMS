import { manifests as UserGroupManifests } from './user-group/manifests.js';
import { manifests as UserStateManifests } from './user-state/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...UserGroupManifests, ...UserStateManifests];
