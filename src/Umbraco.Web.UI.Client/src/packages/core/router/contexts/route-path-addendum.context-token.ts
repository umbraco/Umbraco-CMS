import type { UmbRoutePathAddendum } from './route-path-addendum.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ROUTE_PATH_ADDENDUM_CONTEXT = new UmbContextToken<UmbRoutePathAddendum>('UmbRoutePathAddendum');
