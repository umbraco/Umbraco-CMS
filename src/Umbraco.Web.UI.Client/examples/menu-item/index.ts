import { manifests as actionManifests } from './action/manifests.js';
import { manifests as entityManifests } from './entity/manifests.js';
import { manifests as linkManifests } from './link/manifests.js';

export const manifests = [...actionManifests, ...entityManifests, ...linkManifests];
