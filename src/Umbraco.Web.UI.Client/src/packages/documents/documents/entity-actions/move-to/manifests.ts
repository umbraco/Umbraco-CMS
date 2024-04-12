import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_PICKER_MODAL } from '../../modals/document-picker-modal.token.js';
import { UMB_DOCUMENT_TREE_ALIAS, UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_USER_PERMISSION_DOCUMENT_MOVE } from '../../user-permissions/constants.js';
import { UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.Document.MoveTo',
		name: 'Move Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_DOCUMENT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_MOVE],
			},
		],
	},
];

export const manifests = [...entityActions, ...repositoryManifests];
