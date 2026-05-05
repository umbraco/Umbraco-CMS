import { manifests as booleanManifests } from './boolean/manifests.js';
import { manifests as dateTimeManifests } from './date-time/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...booleanManifests, ...dateTimeManifests];
