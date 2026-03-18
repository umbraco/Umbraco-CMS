import type { UmbValueMinimalDisplayCoordinatorContext } from './value-minimal-display-coordinator.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT =
	new UmbContextToken<UmbValueMinimalDisplayCoordinatorContext>('UmbValueMinimalDisplayCoordinator');
