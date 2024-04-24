import { manifests as mediaSectionManifests } from './section.manifests.js';
import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';

import './media/components/index.js';

export const manifests: Array<ManifestTypes> = [...mediaSectionManifests, ...mediaManifests, ...mediaTypesManifests];
