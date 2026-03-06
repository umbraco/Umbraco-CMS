import { manifests as scheduleManifests } from './has-schedule/manifests.js';
import { manifests as pendingChangesManifests } from './has-pending-changes/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...scheduleManifests, ...pendingChangesManifests];
