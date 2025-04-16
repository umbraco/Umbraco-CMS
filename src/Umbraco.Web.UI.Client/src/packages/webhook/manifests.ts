import { manifests as webhookDeliveryManifests } from './webhook-delivery/manifests.js';
import { manifests as webhookEventManifests } from './webhook-event/manifests.js';
import { manifests as webhookManifests } from './webhook/manifests.js';
import { manifests as webhookRootManifests } from './webhook-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...webhookDeliveryManifests,
	...webhookEventManifests,
	...webhookManifests,
	...webhookRootManifests,
];
