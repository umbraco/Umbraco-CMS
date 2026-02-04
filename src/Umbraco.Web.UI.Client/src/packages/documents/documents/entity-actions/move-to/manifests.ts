import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TREE_ALIAS, UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_USER_PERMISSION_DOCUMENT_MOVE } from '../../user-permissions/document/constants.js';
import { UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const UMB_DOCUMENT_MOVE_SELECTABLE_FILTER_PROVIDER_ALIAS = 'Umb.MoveSelectableFilterProvider.Document';

export const manifests: Array<UmbExtensionManifest> = [
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
			selectableFilterProviderAlias: UMB_DOCUMENT_MOVE_SELECTABLE_FILTER_PROVIDER_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_MOVE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'moveSelectableFilterProvider',
		alias: UMB_DOCUMENT_MOVE_SELECTABLE_FILTER_PROVIDER_ALIAS,
		name: 'Document Move Selectable Filter Provider',
		api: () => import('./document-move-selectable-filter.provider.js'),
	},
	...repositoryManifests,
];
