import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH } from '../../../user-permissions/document/constants.js';
import { UMB_DOCUMENT_UNPUBLISH_META } from './meta.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'contentUnpublish',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		weight: 500,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: UMB_DOCUMENT_UNPUBLISH_META,
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
