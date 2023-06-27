import { manifests as mediaSectionManifests } from './section.manifests.js';
import { manifests as mediaMenuManifests } from './menu.manifests.js';
import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';

export const manifests = [...mediaSectionManifests, ...mediaMenuManifests, ...mediaManifests, ...mediaTypesManifests];
