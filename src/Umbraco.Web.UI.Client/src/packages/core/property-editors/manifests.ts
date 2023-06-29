import { manifests as propertyEditorSchemaManifests } from './schemas/manifests.js';
import { manifests as propertyEditorUIManifests } from './uis/manifests.js';

export const manifests = [...propertyEditorSchemaManifests, ...propertyEditorUIManifests];
