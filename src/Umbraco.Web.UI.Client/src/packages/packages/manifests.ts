import { manifests as repositoryManifests } from './package/repository/manifests.js';
import { manifests as packageBuilderManifests } from './package-builder/manifests.js';
import { manifests as packageRepoManifests } from './package-repo/manifests.js';
import { manifests as packageSectionManifests } from './package-section/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...packageBuilderManifests,
	...packageRepoManifests,
	...packageSectionManifests,
];
