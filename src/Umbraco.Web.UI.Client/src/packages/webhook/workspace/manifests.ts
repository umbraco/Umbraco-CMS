import { manifests as webhookManifests } from './webhook/manifests.js';
import { manifests as webhookRootManifests } from './webhook-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...webhookManifests, ...webhookRootManifests];
