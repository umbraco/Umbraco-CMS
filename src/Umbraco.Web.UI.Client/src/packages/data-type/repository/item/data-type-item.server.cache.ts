import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const dataTypeItemCache = new UmbManagementApiItemDataCache<DataTypeItemResponseModel>();

export { dataTypeItemCache };
