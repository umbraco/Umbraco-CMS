import type { DocumentBlueprintItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const documentBlueprintItemCache = new UmbManagementApiItemDataCache<DocumentBlueprintItemResponseModel>();

export { documentBlueprintItemCache };
