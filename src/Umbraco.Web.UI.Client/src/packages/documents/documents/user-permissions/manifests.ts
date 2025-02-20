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
import { manifests as conditionManifests } from './conditions/manifests.js';
import type {
	ManifestGranularUserPermission,
	ManifestEntityUserPermission,
} from '@umbraco-cms/backoffice/user-permission';

const permissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Read'],
			label: '#actions_browse',
			description: '#actionDescriptions_browse',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT,
		name: 'Create Document Blueprint User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.CreateBlueprint'],
			label: '#actions_createblueprint',
			description: '#actionDescriptions_createblueprint',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DELETE,
		name: 'Delete Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Delete'],
			label: '#actions_delete',
			description: '#actionDescriptions_delete',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CREATE,
		name: 'Create Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Create'],
			label: '#actions_create',
			description: '#actionDescriptions_create',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS,
		name: 'Document Notifications User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Notifications'],
			label: '#actions_notify',
			description: '#actionDescriptions_notify',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
		name: 'Publish Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Publish'],
			label: '#actions_publish',
			description: '#actionDescriptions_publish',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS,
		name: 'Document Permissions User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Permissions'],
			label: '#actions_setPermissions',
			description: '#actionDescriptions_rights',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH,
		name: 'Unpublish Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Unpublish'],
			label: '#actions_unpublish',
			description: '#actionDescriptions_unpublish',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_UPDATE,
		name: 'Update Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Update'],
			label: '#actions_update',
			description: '#actionDescriptions_update',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_DUPLICATE,
		name: 'Duplicate Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Duplicate'],
			label: '#actions_copy',
			description: '#actionDescriptions_copy',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_MOVE,
		name: 'Move Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Move'],
			label: '#actions_move',
			description: '#actionDescriptions_move',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_SORT,
		name: 'Sort Document User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Sort'],
			label: '#actions_sort',
			description: '#actionDescriptions_sort',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES,
		name: 'Document Culture And Hostnames User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.CultureAndHostnames'],
			label: '#actions_assigndomain',
			description: '#actionDescriptions_assignDomain',
			group: 'administration',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS,
		name: 'Document Public Access User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.PublicAccess'],
			label: '#actions_protect',
			description: '#actionDescriptions_protect',
			group: 'administration',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_ROLLBACK,
		name: 'Document Rollback User Permission',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			verbs: ['Umb.Document.Rollback'],
			label: '#actions_rollback',
			description: '#actionDescriptions_rollback',
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
			label: '#user_granularRightsLabel',
			description: '{#user_granularRightsDescription}',
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...permissions,
	...granularPermissions,
	...conditionManifests,
];
