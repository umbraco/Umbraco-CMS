import type { UmbEntryDataValueVariantsController } from '../controller/entry-data-value-variants.controller.js';
import type { UmbEntryWithVariantsDetailModel } from '../types.js';
import type { UmbEntryWorkspaceContext } from './entry-workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbEntryWithVariantsWorkspaceContext<
	T extends UmbEntryWithVariantsDetailModel = UmbEntryWithVariantsDetailModel,
>
	extends UmbVariantDatasetWorkspaceContext<T['variants'][0]>, UmbEntryWorkspaceContext {
	// Data:
	readonly valueVariants: UmbEntryDataValueVariantsController<T>;
}
