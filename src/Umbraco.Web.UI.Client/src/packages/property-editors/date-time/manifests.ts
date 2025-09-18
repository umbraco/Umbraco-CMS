import { manifests as dateTimeManifests } from './date-time-picker/manifests.js';
import { manifests as dateTimeWithTimeZoneManifests } from './date-time-with-time-zone-picker/manifests.js';
import { manifests as dateOnlyManifests } from './date-only-picker/manifests.js';
import { manifests as timeOnlyManifests } from './time-only-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...dateTimeManifests,
	...dateTimeWithTimeZoneManifests,
	...dateOnlyManifests,
	...timeOnlyManifests,
];
