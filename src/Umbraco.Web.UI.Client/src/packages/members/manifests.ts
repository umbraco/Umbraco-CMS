import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberManifests } from './member/manifests.js';
import { manifests as memberSectionManifests } from './member-section/manifests.js';
import { manifests as memberTypeManifests } from './member-type/manifests.js';

import './member/components/index.js';
import './member-group/components/index.js';

export const manifests = [
	...memberGroupManifests,
	...memberManifests,
	...memberSectionManifests,
	...memberTypeManifests,
];
