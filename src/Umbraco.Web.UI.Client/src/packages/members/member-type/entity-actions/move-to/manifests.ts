import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_MEMBER_TYPE_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.MemberType.MoveTo',
		name: 'Move Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS,
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
			searchProviderAlias: UMB_MEMBER_TYPE_SEARCH_PROVIDER_ALIAS,
			foldersOnly: true,
		},
	},
	...repositoryManifests,
];
