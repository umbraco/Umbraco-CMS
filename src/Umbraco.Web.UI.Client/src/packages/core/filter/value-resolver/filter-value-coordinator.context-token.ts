import type { UmbFilterValueCoordinatorContext } from './filter-value-coordinator.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FILTER_VALUE_COORDINATOR_CONTEXT = new UmbContextToken<UmbFilterValueCoordinatorContext>(
	'UmbFilterValueCoordinatorContext',
);
