import { manifests as contentSectionManifests } from './content/manifests';
import { manifests as mediaSectionManifests } from './media/manifests';
import { manifests as memberSectionManifests } from './members/manifests';
import { manifests as packageSectionManifests } from './packages/manifests';
import { manifests as settingsSectionManifests } from './settings/manifests';
import { manifests as translationSectionManifests } from './translation/manifests';
import { manifests as userSectionManifests } from './users/manifests';

import type { ManifestDashboard, ManifestDashboardCollection, ManifestSection, ManifestSectionView } from '@umbraco-cms/models';

export const manifests: Array<ManifestSection | ManifestDashboardCollection | ManifestDashboard | ManifestSectionView> = [
	...contentSectionManifests,
	...mediaSectionManifests,
	...memberSectionManifests,
	...packageSectionManifests,
	...settingsSectionManifests,
	...translationSectionManifests,
	...userSectionManifests,
];
