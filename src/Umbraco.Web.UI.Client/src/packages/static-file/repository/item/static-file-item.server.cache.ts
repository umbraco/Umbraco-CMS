import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const staticFileItemCache = new UmbManagementApiItemDataCache<StaticFileItemResponseModel>();

export { staticFileItemCache };
