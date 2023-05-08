import { manifests as repositoryManifests } from './repository/manifests';

import { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [...repositoryManifests];

export const onInit: UmbEntrypointOnInit = (host, extensionRegistry) => {
	console.log('tags registrer');
	extensionRegistry.registerMany(manifests);
};
