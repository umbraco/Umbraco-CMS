import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataRuntimeCache } from '@umbraco-cms/backoffice/management-api';

const documentTypeDetailCache = new UmbManagementApiDetailDataRuntimeCache<DocumentTypeResponseModel>();

export { documentTypeDetailCache };
