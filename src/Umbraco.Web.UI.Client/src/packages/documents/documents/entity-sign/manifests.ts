import { manifests as protectedManifest } from './is-protected/manifest.js';
import { manifests as scheduleManifests } from './has-schedule/manifests.js';
import { manifests as pendingChangesManifests } from './has-pending-changes/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	protectedManifest,
	...scheduleManifests,
	...pendingChangesManifests,
];
