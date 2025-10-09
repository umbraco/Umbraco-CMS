import type { PartialViewItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const partialViewItemCache = new UmbManagementApiItemDataCache<PartialViewItemResponseModel>();

export { partialViewItemCache };
