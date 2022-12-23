// TODO: temp file until we have a way to register from each extension

import { manifests as contentSectionManifests } from './test/documents/content-section/manifests';
import { manifests as mediaSectionManifests } from './test/media/media-section/manifests';
import { manifests as memberSectionManifests } from './test/members/members-section/manifests';
import { manifests as packageSectionManifests } from './test/packages/packages-section/manifests';
import { manifests as settingsSectionManifests } from './test/core/settings-section/manifests';
import { manifests as translationSectionManifests } from './test/translation/translation-section/manifests';
import { manifests as userSectionManifests } from '../auth/users-section/manifests';

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
		...translationSectionManifests,
		...userSectionManifests,
	];
