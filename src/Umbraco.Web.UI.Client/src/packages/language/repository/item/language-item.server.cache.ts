import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const languageItemCache = new UmbManagementApiItemDataCache<LanguageItemResponseModel>();

export { languageItemCache };
