import { manifests as webhookManifests } from './webhook/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...webhookManifests];
