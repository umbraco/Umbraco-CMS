import {
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
} from '../../constants.js';
import { UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../repository/detail/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'auditLogAction',
		kind: 'contentRollback',
		alias: 'Umb.AuditLogAction.Document.Rollback',
		name: 'Document Audit Log Rollback Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			rollbackRepositoryAlias: UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_DOCUMENT_ROLLBACK],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
