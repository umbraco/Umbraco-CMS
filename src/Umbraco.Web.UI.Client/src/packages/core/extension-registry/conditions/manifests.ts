import type { ManifestTypes } from '../models/index.js';
import { manifest as menuAliasConditionManifest } from './menu-alias.condition.js';
import { manifest as multipleAppLanguagesConditionManifest } from './multiple-app-languages.condition.js';
import { manifest as sectionAliasConditionManifest } from './section-alias.condition.js';
import { manifest as switchConditionManifest } from './switch.condition.js';

export const manifests: Array<ManifestTypes> = [
	menuAliasConditionManifest,
	multipleAppLanguagesConditionManifest,
	sectionAliasConditionManifest,
	switchConditionManifest,
];
