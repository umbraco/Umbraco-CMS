import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiRuntimeCache } from '@umbraco-cms/backoffice/management-api';

const cache = new UmbManagementApiRuntimeCache<DocumentTypeResponseModel>();

export { cache };
