import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberManifests } from './member/manifests.js';
import { manifests as memberSectionManifests } from './section.manifests.js';
import { manifests as memberTypeManifests } from './member-type/manifests.js';
import { manifests as memberWelcomeDashboardManifests } from './welcome-dashboard/manifests.js';
import { manifests as menuSectionManifests } from './menu.manifests.js';

import './member/components/index.js';

export const manifests = [
	...memberGroupManifests,
	...memberManifests,
	...memberSectionManifests,
	...memberTypeManifests,
	...memberWelcomeDashboardManifests,
	...menuSectionManifests,
];
