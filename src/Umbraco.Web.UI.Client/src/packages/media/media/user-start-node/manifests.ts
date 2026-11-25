import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...conditionManifests, ...valueSummaryManifests];
