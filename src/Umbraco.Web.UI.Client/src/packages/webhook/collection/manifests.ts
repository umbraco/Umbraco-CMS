import { manifests as webhooksManifests } from './webhooks/manifests.js';
import { manifests as deliveriesManifests } from './deliveries/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...webhooksManifests,
	...deliveriesManifests,
];
