import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type {
	ManifestUserGranularPermission,
	ManifestEntityUserPermission,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_PERMISSION_DOCUMENT_CREATE = 'Umb.UserPermission.Document.Create';
export const UMB_USER_PERMISSION_DOCUMENT_READ = 'Umb.UserPermission.Document.Read';
export const UMB_USER_PERMISSION_DOCUMENT_UPDATE = 'Umb.UserPermission.Document.Update';
export const UMB_USER_PERMISSION_DOCUMENT_DELETE = 'Umb.UserPermission.Document.Delete';
export const UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT = 'Umb.UserPermission.Document.CreateBlueprint';
export const UMB_USER_PERMISSION_DOCUMENT_NOTIFICATIONS = 'Umb.UserPermission.Document.Notifications';
export const UMB_USER_PERMISSION_DOCUMENT_PUBLISH = 'Umb.UserPermission.Document.Publish';
export const UMB_USER_PERMISSION_DOCUMENT_PERMISSIONS = 'Umb.UserPermission.Document.Permissions';
export const UMB_USER_PERMISSION_DOCUMENT_UNPUBLISH = 'Umb.UserPermission.Document.Unpublish';
export const UMB_USER_PERMISSION_DOCUMENT_COPY = 'Umb.UserPermission.Document.Copy';
export const UMB_USER_PERMISSION_DOCUMENT_MOVE = 'Umb.UserPermission.Document.Move';
export const UMB_USER_PERMISSION_DOCUMENT_SORT = 'Umb.UserPermission.Document.Sort';
export const UMB_USER_PERMISSION_DOCUMENT_CULTURE_AND_HOSTNAMES = 'Umb.UserPermission.Document.CultureAndHostnames';
export const UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS = 'Umb.UserPermission.Document.PublicAccess';
export const UMB_USER_PERMISSION_DOCUMENT_ROLLBACK = 'Umb.UserPermission.Document.Rollback';

const permissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_READ,
		name: 'Read Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verb: 'Umb.Document.Read',
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
			verb: 'Umb.Document.CreateBlueprint',
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
			verb: 'Umb.Document.Delete',
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
			verb: 'Umb.Document.Create',
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
			verb: 'Umb.Document.Notifications',
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
			verb: 'Umb.Document.Publish',
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
			verb: 'Umb.Document.Permissions',
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
			verb: 'Umb.Document.Unpublish',
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
			verb: 'Umb.Document.Update',
			labelKey: 'actions_update',
			descriptionKey: 'actionDescriptions_update',
		},
	},
	{
		type: 'entityUserPermission',
		alias: UMB_USER_PERMISSION_DOCUMENT_COPY,
		name: 'Copy Document User Permission',
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			verb: 'Umb.Document.Copy',
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
			verb: 'Umb.Document.Move',
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
			verb: 'Umb.Document.Sort',
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
			verb: 'Umb.Document.CultureAndHostnames',
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
			verb: 'Umb.Document.PublicAccess',
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
			verb: 'Umb.Document.Rollback',
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
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
		},
	},
];

export const manifests = [...repositoryManifests, ...permissions, ...granularPermissions];
