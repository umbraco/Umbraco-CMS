import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_TREE_ALIAS,
	UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
} from '../../constants.js';
import { UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_MEDIA_TYPE_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityAction.MediaType.DuplicateTo',
		name: 'Duplicate Document To Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_MEDIA_TYPE_TREE_ALIAS,
			treeRepositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
			searchProviderAlias: UMB_MEDIA_TYPE_SEARCH_PROVIDER_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
