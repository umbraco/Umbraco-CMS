import { manifests as globalContextManifests } from './global-context/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as queryManifests } from './query/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as indexDetailBoxManifests } from './index-detail-box/manifests.js';
import { manifests as rootManifests } from './root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...globalContextManifests,
	...conditionManifests,
	...queryManifests,
	...collectionManifests,
	...detailManifests,
	...workspaceManifests,
	...indexDetailBoxManifests,
	...rootManifests,
];
