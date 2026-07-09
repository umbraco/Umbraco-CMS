import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as actionManifests } from './action/manifests.js';
import { manifests as pickerModalManifests } from './collection-item-picker-modal/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as filterManifests } from './filter/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as viewManifests } from './view/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-view/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...actionManifests,
	...conditionManifests,
	...filterManifests,
	...menuManifests,
	...pickerModalManifests,
	...viewManifests,
	...workspaceViewManifests,
];
