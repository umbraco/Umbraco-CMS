import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/index.js';
import { UMB_USER_PERMISSION_DOCUMENT_DUPLICATE } from '../../user-permissions/constants.js';
import { UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityAction.Document.DuplicateTo',
		name: 'Duplicate Document To Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DUPLICATE],
			},
		],
	},
];

export const manifests = [...entityActions, ...repositoryManifests];
