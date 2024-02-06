import { manifests as memberSectionManifests } from './section.manifests.js';
import { manifests as menuSectionManifests } from './menu.manifests.js';
import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberTypeManifests } from './member-types/manifests.js';
import { manifests as memberManifests } from './member/manifests.js';

import './member/components/index.js';

export const manifests = [
	...memberSectionManifests,
	...menuSectionManifests,
	...memberGroupManifests,
	...memberTypeManifests,
	...memberManifests,
];
