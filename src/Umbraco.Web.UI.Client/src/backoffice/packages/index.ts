import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as packageBuilderManifests } from './package-builder/manifests';
import { manifests as packageRepoManifests } from './package-repo/manifests';
import { manifests as packageSectionManifests } from './package-section/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...repositoryManifests,
	...packageBuilderManifests,
	...packageRepoManifests,
	...packageSectionManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
