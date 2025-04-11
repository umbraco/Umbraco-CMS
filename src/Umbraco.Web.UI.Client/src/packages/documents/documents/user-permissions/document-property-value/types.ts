import type { UmbDocumentPropertyValueUserPermissionType } from './user-permission.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentPropertyValueUserPermissionModel extends UmbUserPermissionModel {
	userPermissionType: UmbDocumentPropertyValueUserPermissionType;
	documentType: UmbReferenceByUnique;
	propertyType: UmbReferenceByUnique;
}
