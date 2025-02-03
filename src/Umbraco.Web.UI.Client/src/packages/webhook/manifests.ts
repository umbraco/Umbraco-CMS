import { manifests as deliveryManifests } from './webhook-delivery/manifests.js';
import { manifests as eventManifests } from './webhook-event/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as webhookManifests } from './webhook/manifests.js';
import { manifests as webhookRootManifests } from './webhook-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...deliveryManifests,
	...eventManifests,
	...repositoryManifests,
	...webhookManifests,
	...webhookRootManifests,
];
