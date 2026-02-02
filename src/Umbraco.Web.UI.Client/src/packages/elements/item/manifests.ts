import { manifests as itemRef } from './item-ref/manifests.js';
import { manifests as repository } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...itemRef, ...repository];
