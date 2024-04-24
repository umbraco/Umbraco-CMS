import { manifests as modalManifests } from './modal/manifests.js';
import { manifest as duplicateToKindManifest } from './duplicate-to.action.kind.js';

export const manifests: Array<ManifestTypes> = [duplicateToKindManifest, ...modalManifests];
