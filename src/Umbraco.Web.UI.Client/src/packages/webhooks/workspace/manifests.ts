import { manifests as webhookRootManifests } from './webhooks-root/manifests.js';
import { manifests as webhookManifests } from './webhooks/manifests.js';

export const manifests = [...webhookRootManifests, ...webhookManifests];
