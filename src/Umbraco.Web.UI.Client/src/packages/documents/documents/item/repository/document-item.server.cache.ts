import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const documentItemCache = new UmbManagementApiItemDataCache<DocumentItemResponseModel>();

export { documentItemCache };
