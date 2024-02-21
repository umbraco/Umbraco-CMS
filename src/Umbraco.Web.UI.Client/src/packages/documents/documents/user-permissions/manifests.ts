import {
	UMB_USER_PERMISSION_DOCUMENT_READ,
	UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
	UMB_USER_PERMISSION_DOCUMENT_SEND_FOR_APPROVAL,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
	UMB_USER_PERMISSION_DOCUMENT_COPY,
	UMB_USER_PERMISSION_DOCUMENT_MOVE,
	UMB_USER_PERMISSION_DOCUMENT_SORT,
	UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
	UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
	UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
} from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type {
	ManifestUserGranularPermission,
	ManifestUserPermission,
} from '@umbraco-cms/backoffice/extension-registry';

const permissions: Array<ManifestUserPermission> = [
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_browse',
			descriptionKey: 'actionDescriptions_browse',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
		name: 'Create Document Blueprint User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_createblueprint',
			descriptionKey: 'actionDescriptions_createblueprint',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DELETE,
		name: 'Delete Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_delete',
			descriptionKey: 'actionDescriptions_delete',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE,
		name: 'Create Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_create',
			descriptionKey: 'actionDescriptions_create',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
		name: 'Document Notifications User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_notify',
			descriptionKey: 'actionDescriptions_notify',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		name: 'Publish Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_publish',
			descriptionKey: 'actionDescriptions_publish',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
		name: 'Document Permissions User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_setPermissions',
			descriptionKey: 'actionDescriptions_rights',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SEND_FOR_APPROVAL,
		name: 'Send Document For Approval User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_sendtopublish',
			descriptionKey: 'actionDescriptions_sendtopublish',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
		name: 'Unpublish Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_unpublish',
			descriptionKey: 'actionDescriptions_unpublish',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UPDATE,
		name: 'Update Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_update',
			descriptionKey: 'actionDescriptions_update',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_COPY,
		name: 'Copy Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_copy',
			descriptionKey: 'actionDescriptions_copy',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_MOVE,
		name: 'Move Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_move',
			descriptionKey: 'actionDescriptions_move',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SORT,
		name: 'Sort Document User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_sort',
			descriptionKey: 'actionDescriptions_sort',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
		name: 'Document Culture And Hostnames User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_assigndomain',
			descriptionKey: 'actionDescriptions_assignDomain',
			group: 'administration',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
		name: 'Document Public Access User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_protect',
			descriptionKey: 'actionDescriptions_protect',
			group: 'administration',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
		name: 'Document Rollback User Permission',
		meta: {
			entityType: 'document',
			labelKey: 'actions_rollback',
			descriptionKey: 'actionDescriptions_rollback',
			group: 'administration',
		},
	},
];

export const granularPermissions: Array<ManifestUserGranularPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document',
		name: 'Document Granular User Permission',
		js: () => import('../components/input-document-granular-permission/input-document-granular-permission.element.js'),
		meta: {
			entityType: 'document',
		},
	},
];

export const manifests = [...repositoryManifests, ...permissions, ...granularPermissions];
