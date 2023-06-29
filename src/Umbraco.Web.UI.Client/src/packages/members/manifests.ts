import { manifests as memberSectionManifests } from './section.manifests.js';
import { manifests as menuSectionManifests } from './menu.manifests.js';
import { manifests as memberGroupManifests } from './member-groups/manifests.js';
import { manifests as memberTypeManifests } from './member-types/manifests.js';
import { manifests as memberManifests } from './members/manifests.js';

export const manifests = [
	...memberSectionManifests,
	...menuSectionManifests,
	...memberGroupManifests,
	...memberTypeManifests,
	...memberManifests,
];
