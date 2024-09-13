import type { ManifestTypes } from '../models/index.js';
import { manifest as multipleAppLanguagesConditionManifest } from './multiple-app-languages.condition.js';
import { manifest as switchConditionManifest } from './switch.condition.js';

export const manifests: Array<ManifestTypes> = [multipleAppLanguagesConditionManifest, switchConditionManifest];
