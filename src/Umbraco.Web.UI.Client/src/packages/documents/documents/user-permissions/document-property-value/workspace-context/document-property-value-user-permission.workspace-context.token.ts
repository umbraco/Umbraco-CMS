import type { UmbDocumentPropertyValueUserPermissionWorkspaceContext } from './document-property-value-user-permission.workspace-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_WORKSPACE_CONTEXT =
	new UmbContextToken<UmbDocumentPropertyValueUserPermissionWorkspaceContext>(
		'UmbDocumentPropertyValueUserPermissionWorkspaceContext',
	);
