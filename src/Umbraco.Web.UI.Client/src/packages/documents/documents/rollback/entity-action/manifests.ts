import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_ROLLBACK } from '../../user-permissions/index.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/index.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Rollback',
		name: 'Rollback Document Entity Action',
		weight: 500,
		api: () => import('./rollback.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-history',
			label: '#actions_rollback',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_ROLLBACK],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
			/* Currently the rollback is tightly coupled to the workspace contexts so we only allow it to show up
			 In the document workspace. */
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
