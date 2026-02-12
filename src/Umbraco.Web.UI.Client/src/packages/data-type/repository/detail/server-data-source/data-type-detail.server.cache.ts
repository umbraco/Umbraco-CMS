import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataCache } from '@umbraco-cms/backoffice/management-api';

// We use a singleton so we can share the cache across the application
const dataTypeDetailCache = new UmbManagementApiDetailDataCache<DataTypeResponseModel>();

export { dataTypeDetailCache };
