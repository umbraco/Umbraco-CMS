import type {
	DocumentPermissionPresentationModel,
	UnknownTypePermissionPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export function isDocumentUserPermission(
	permission: DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel,
): permission is DocumentPermissionPresentationModel {
	return (permission as DocumentPermissionPresentationModel).$type === 'DocumentPermissionPresentationModel';
}
