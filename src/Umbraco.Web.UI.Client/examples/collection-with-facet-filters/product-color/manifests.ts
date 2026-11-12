import { manifests as itemRepositoryManifests } from './repository/item/manifests.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...itemRepositoryManifests, ...valueSummaryManifests];
