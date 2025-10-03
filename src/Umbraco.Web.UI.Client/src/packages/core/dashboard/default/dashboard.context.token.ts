import type { UmbDashboardContext } from './dashboard.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DASHBOARD_CONTEXT = new UmbContextToken<UmbDashboardContext>('UmbDashboardContext');
