import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataCache } from '@umbraco-cms/backoffice/management-api';

const documentTypeDetailCache = new UmbManagementApiDetailDataCache<DocumentTypeResponseModel>();

export { documentTypeDetailCache };
