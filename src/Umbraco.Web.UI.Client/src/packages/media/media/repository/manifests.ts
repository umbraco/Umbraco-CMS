import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as validationManifests } from './validation/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...detailManifests, ...itemManifests, ...validationManifests];
