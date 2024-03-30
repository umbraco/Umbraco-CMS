import { manifests as propertyEditors } from './property-editors/manifests.js';
import { manifests as plugins } from './plugins/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';

export const manifests = [...propertyEditors, ...plugins, ...modalManifests];
