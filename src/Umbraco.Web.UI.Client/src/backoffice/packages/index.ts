import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as packageBuilderManifests } from './package-builder/manifests';
import { manifests as packageRepoManifests } from './package-repo/manifests';
import { manifests as packageSectionManifests } from './package-section/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...repositoryManifests,
	...packageBuilderManifests,
	...packageRepoManifests,
	...packageSectionManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
