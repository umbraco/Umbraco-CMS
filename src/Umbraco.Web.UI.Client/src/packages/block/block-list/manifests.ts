import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as workspaceManifests } from './workspace/views/manifests.js';

export const manifests = [...workspaceManifests, ...propertyEditorManifests];
