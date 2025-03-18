import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentPropertyValueUserPermissionModel extends UmbUserPermissionModel {
	permissionType: 'document-property-value';
	documentType: UmbReferenceByUnique;
	propertyType: UmbReferenceByUnique;
}
