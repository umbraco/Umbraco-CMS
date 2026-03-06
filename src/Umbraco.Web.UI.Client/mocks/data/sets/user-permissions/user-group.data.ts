import type { UmbMockUserGroupModel } from '../../types/mock-data-set.types.js';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
	UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
	UMB_USER_PERMISSION_DOCUMENT_MOVE,
	UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
	UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
	UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_READ,
	UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
	UMB_USER_PERMISSION_DOCUMENT_SORT,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '@umbraco-cms/backoffice/document';

export const data: Array<UmbMockUserGroupModel> = [
	{
		id: 'user-group-administrators-id',
		name: 'Administrators',
		alias: 'admin',
		description: 'Administrators have full access to all settings and features within the CMS.',
		icon: 'icon-medal',
		fallbackPermissions: [
			UMB_USER_PERMISSION_DOCUMENT_READ,
			UMB_USER_PERMISSION_DOCUMENT_CREATE,
			UMB_USER_PERMISSION_DOCUMENT_UPDATE,
			UMB_USER_PERMISSION_DOCUMENT_DELETE,
			UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
			UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
			UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
			UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
			UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
			UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
			UMB_USER_PERMISSION_DOCUMENT_MOVE,
			UMB_USER_PERMISSION_DOCUMENT_SORT,
			UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
			UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
			UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
			UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
			UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
		],
		permissions: [
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-document-id',
				},
				verbs: ['Umb.Document.Read'],
			},
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-2-document-id',
				},
				verbs: ['Umb.Document.Create', 'Umb.Document.Read'],
			},
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-2-2-document-id',
				},
				verbs: ['Umb.Document.Delete', 'Umb.Document.Read'],
			},
		],
		sections: [
			UMB_CONTENT_SECTION_ALIAS,
			'Umb.Section.Media',
			'Umb.Section.Settings',
			'Umb.Section.Members',
			'Umb.Section.Packages',
			'Umb.Section.Translation',
			'Umb.Section.Users',
		],
		languages: [],
		hasAccessToAllLanguages: true,
		documentRootAccess: true,
		mediaRootAccess: true,
		aliasCanBeChanged: false,
		isDeletable: false,
		flags: [],
	},
];
