import type { DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const dictionaryItemCache = new UmbManagementApiItemDataCache<DictionaryItemItemResponseModel>();

export { dictionaryItemCache };
