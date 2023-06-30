import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';

export const manifests = [...repositoryManifests, ...propertyEditorManifests];
