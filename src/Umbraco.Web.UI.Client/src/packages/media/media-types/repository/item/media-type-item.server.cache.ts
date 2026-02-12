import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const mediaTypeItemCache = new UmbManagementApiItemDataCache<MediaTypeItemResponseModel>();

export { mediaTypeItemCache };
