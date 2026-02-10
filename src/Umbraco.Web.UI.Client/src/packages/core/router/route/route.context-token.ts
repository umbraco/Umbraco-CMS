import type { UmbRouteContext } from './route.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ROUTE_CONTEXT = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
