import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
	UMB_ELEMENT_ROLLBACK_REPOSITORY_ALIAS,
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_ROLLBACK,
} from '../../constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'auditLogAction',
		kind: 'contentRollback',
		alias: 'Umb.AuditLogAction.Element.Rollback',
		name: 'Element Audit Log Rollback Action',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			rollbackNotificationMessage: '#rollback_elementRolledBack',
			rollbackRepositoryAlias: UMB_ELEMENT_ROLLBACK_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_ROLLBACK],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
