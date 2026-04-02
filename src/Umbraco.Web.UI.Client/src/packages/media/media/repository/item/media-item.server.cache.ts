import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const mediaItemCache = new UmbManagementApiItemDataCache<MediaItemResponseModel>();

export { mediaItemCache };
