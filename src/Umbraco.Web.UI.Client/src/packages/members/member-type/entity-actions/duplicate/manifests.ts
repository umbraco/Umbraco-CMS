import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_DUPLICATE_MEMBER_TYPE_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.MemberType.Duplicate',
		name: 'Duplicate Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_MEMBER_TYPE_REPOSITORY_ALIAS,
			treeRepositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...repositoryManifests];
