import { manifests as dictionarySectionManifests } from './section.manifest.js';
import { manifests as dictionaryManifests } from './dictionary/manifests.js';

export const manifests = [...dictionarySectionManifests, ...dictionaryManifests];
