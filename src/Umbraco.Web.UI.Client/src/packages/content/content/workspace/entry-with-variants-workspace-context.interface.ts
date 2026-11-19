import type { UmbEntryDataValueVariantsController } from '../controller/entry-data-value-variants.controller.js';
import type { UmbEntryWithVariantsDetailModel } from '../types.js';
import type { UmbEntryWorkspaceContext } from './entry-workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

export interface UmbEntryWithVariantsWorkspaceContext<
	T extends UmbEntryWithVariantsDetailModel = UmbEntryWithVariantsDetailModel,
>
	extends UmbVariantDatasetWorkspaceContext<T['variants'][0]>, UmbEntryWorkspaceContext {
	readonly readOnlyGuard: UmbReadOnlyVariantGuardManager;

	// Data:
	readonly valueVariants: UmbEntryDataValueVariantsController<T>;
}
