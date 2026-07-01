import type { MediaTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataCache } from '@umbraco-cms/backoffice/management-api';

const mediaTypeDetailCache = new UmbManagementApiDetailDataCache<MediaTypeResponseModel>();

export { mediaTypeDetailCache };
