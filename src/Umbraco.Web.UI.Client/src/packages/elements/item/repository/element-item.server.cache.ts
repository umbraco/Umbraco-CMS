import type { ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const elementItemCache = new UmbManagementApiItemDataCache<ElementItemResponseModel>();

export { elementItemCache };
