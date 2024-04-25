import { UmbMediaUrlRepository } from './media-url.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_URL_REPOSITORY_ALIAS = 'Umb.Repository.Media.Url';

const urlRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_URL_REPOSITORY_ALIAS,
	name: 'Media Url Repository',
	api: UmbMediaUrlRepository,
};

export const manifests = [urlRepository];
