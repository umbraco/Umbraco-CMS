import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const documentBlueprintFolderItemCache = new UmbManagementApiItemDataCache<FolderItemResponseModel>();

export { documentBlueprintFolderItemCache };
