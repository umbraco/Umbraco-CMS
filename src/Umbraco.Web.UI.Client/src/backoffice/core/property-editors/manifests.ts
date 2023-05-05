import { manifests as propertyEditorModelManifests } from './models/manifests';
import { manifests as propertyEditorUIManifests } from './uis/manifests';
import { manifests as modalManifests } from './modals/manifests';

export const manifests = [...propertyEditorModelManifests, ...propertyEditorUIManifests, ...modalManifests];
