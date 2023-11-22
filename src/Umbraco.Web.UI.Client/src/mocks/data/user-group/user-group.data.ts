import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_READ,
} from '@umbraco-cms/backoffice/document';

export const data: Array<UserGroupResponseModel> = [
	{
		id: 'user-group-administrators-id',
		name: 'Administrators',
		icon: 'icon-medal',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [
			UMB_USER_PERMISSION_DOCUMENT_READ,
			UMB_USER_PERMISSION_DOCUMENT_CREATE,
			UMB_USER_PERMISSION_DOCUMENT_DELETE,
		],
	},
	{
		id: 'user-group-editors-id',
		name: 'Editors',
		icon: 'icon-tools',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'user-group-sensitive-data-id',
		name: 'Sensitive data',
		icon: 'icon-lock',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'user-group-translators-id',
		name: 'Translators',
		icon: 'icon-globe',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'user-group-writers-id',
		name: 'Writers',
		icon: 'icon-edit',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
];
