import { dataSet } from './sets/index.js';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<NewsDashboardItemResponseModel> = dataSet.news;
