import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_READ,
	UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
	UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
	UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
	UMB_USER_PERMISSION_DOCUMENT_MOVE,
	UMB_USER_PERMISSION_DOCUMENT_SORT,
	UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
	UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
	UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
} from './constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type {
	ManifestGranularUserPermission,
	ManifestEntityUserPermission,
} from '@umbraco-cms/backoffice/extension-registry';

const permissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Read'],
			labelKey: 'actions_browse',
			descriptionKey: 'actionDescriptions_browse',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
		name: 'Create Document Blueprint User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.CreateBlueprint'],
			labelKey: 'actions_createblueprint',
			descriptionKey: 'actionDescriptions_createblueprint',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DELETE,
		name: 'Delete Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Delete'],
			labelKey: 'actions_delete',
			descriptionKey: 'actionDescriptions_delete',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE,
		name: 'Create Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Create'],
			labelKey: 'actions_create',
			descriptionKey: 'actionDescriptions_create',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
		name: 'Document Notifications User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Notifications'],
			labelKey: 'actions_notify',
			descriptionKey: 'actionDescriptions_notify',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		name: 'Publish Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Publish'],
			labelKey: 'actions_publish',
			descriptionKey: 'actionDescriptions_publish',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
		name: 'Document Permissions User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Permissions'],
			labelKey: 'actions_setPermissions',
			descriptionKey: 'actionDescriptions_rights',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
		name: 'Unpublish Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Unpublish'],
			labelKey: 'actions_unpublish',
			descriptionKey: 'actionDescriptions_unpublish',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UPDATE,
		name: 'Update Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Update'],
			labelKey: 'actions_update',
			descriptionKey: 'actionDescriptions_update',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
		name: 'Duplicate Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Duplicate'],
			labelKey: 'actions_copy',
			descriptionKey: 'actionDescriptions_copy',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_MOVE,
		name: 'Move Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Move'],
			labelKey: 'actions_move',
			descriptionKey: 'actionDescriptions_move',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SORT,
		name: 'Sort Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Sort'],
			labelKey: 'actions_sort',
			descriptionKey: 'actionDescriptions_sort',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
		name: 'Document Culture And Hostnames User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.CultureAndHostnames'],
			labelKey: 'actions_assigndomain',
			descriptionKey: 'actionDescriptions_assignDomain',
			group: 'administration',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
		name: 'Document Public Access User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.PublicAccess'],
			labelKey: 'actions_protect',
			descriptionKey: 'actionDescriptions_protect',
			group: 'administration',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
		name: 'Document Rollback User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verbs: ['Umb.Document.Rollback'],
			labelKey: 'actions_rollback',
			descriptionKey: 'actionDescriptions_rollback',
			group: 'administration',
		},
	},
];

export const granularPermissions: Array<ManifestGranularUserPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document',
		name: 'Document Granular User Permission',
		element: () =>
			import('./input-document-granular-user-permission/input-document-granular-user-permission.element.js'),
		meta: {
			schemaType: 'DocumentPermissionPresentationModel',
			label: 'Documents',
			description: 'Assign permissions to specific documents',
		},
	},
];

export const manifests = [...repositoryManifests, ...permissions, ...granularPermissions];
