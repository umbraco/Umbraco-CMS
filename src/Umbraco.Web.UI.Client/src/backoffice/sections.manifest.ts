// TODO: temp file until we have a way to register from each extension

import { manifests as userSectionManifests } from '../auth/users-section/manifests';
import { manifests as contentSectionManifests } from './documents/content-section/content-section.manifest';
import { manifests as mediaSectionManifests } from './media/media-section/manifests';
import { manifests as memberSectionManifests } from './members/members-section/manifests';
import { manifests as packageSectionManifests } from './packages/packages-section/manifests';
import { manifests as settingsSectionManifests } from './core/settings-section/manifests';

import type {
	ManifestDashboard,
	ManifestDashboardCollection,
	ManifestSection,
	ManifestSectionView,
} from '@umbraco-cms/models';

export const manifests: Array<ManifestSection | ManifestDashboardCollection | ManifestDashboard | ManifestSectionView> =
	[
		...contentSectionManifests,
		...mediaSectionManifests,
		...memberSectionManifests,
		...packageSectionManifests,
		...settingsSectionManifests,
		...userSectionManifests,
	];
