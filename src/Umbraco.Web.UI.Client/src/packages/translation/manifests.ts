import { manifests as translationSectionManifests } from './section.manifest.js';
import { manifests as dictionaryManifests } from './dictionary/manifests.js';

export const manifests = [...translationSectionManifests, ...dictionaryManifests];
