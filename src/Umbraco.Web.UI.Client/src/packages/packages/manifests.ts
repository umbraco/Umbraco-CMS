import { manifests as repositoryManifests } from './package/repository/manifests.js';
import { manifests as packageBuilderManifests } from './package-builder/manifests.js';
import { manifests as packageRepoManifests } from './package-repo/manifests.js';
import { manifests as packageSectionManifests } from './package-section/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...repositoryManifests,
	...packageBuilderManifests,
	...packageRepoManifests,
	...packageSectionManifests,
];
