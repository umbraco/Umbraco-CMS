import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as packageBuilderManifests } from './package-builder/manifests';
import { manifests as packageRepoManifests } from './package-repo/manifests';
import { manifests as packageSectionManifests } from './package-section/manifests';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [
	...repositoryManifests,
	...packageBuilderManifests,
	...packageRepoManifests,
	...packageSectionManifests,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
