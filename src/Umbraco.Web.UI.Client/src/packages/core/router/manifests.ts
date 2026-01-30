import { manifests as UmbIsRoutableContextConditionManifests } from './conditions/is-routable-context/manifests.js';
import { manifests as UmbIsNotRoutableContextConditionManifests } from './conditions/is-not-routable-context/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...UmbIsRoutableContextConditionManifests,
	...UmbIsNotRoutableContextConditionManifests,
];
