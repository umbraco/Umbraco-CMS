import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';

export interface UmbDocumentPropertyValueUserPermissionModel extends UmbUserPermissionModel {
	documentType: UmbReferenceByUnique;
	propertyType: UmbReferenceByUnique;
}
