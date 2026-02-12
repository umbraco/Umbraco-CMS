import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const documentTypeItemCache = new UmbManagementApiItemDataCache<DocumentTypeItemResponseModel>();

export { documentTypeItemCache };
