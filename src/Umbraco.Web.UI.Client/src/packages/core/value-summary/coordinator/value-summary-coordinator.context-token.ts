import type { UmbValueSummaryCoordinatorContext } from './value-summary-coordinator.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT =
	new UmbContextToken<UmbValueSummaryCoordinatorContext>('UmbValueSummaryCoordinator');
