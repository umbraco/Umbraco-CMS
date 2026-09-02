import type { UmbEntryWorkspaceContext } from './entry-workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContext } from './variant-dataset-workspace-context.interface.js';
import type {
	UmbEntryDataValueVariantsController,
	UmbEntryWithVariantsDetailModel,
} from '@umbraco-cms/backoffice/content';

export interface UmbEntryWithVariantsWorkspaceContext<
	T extends UmbEntryWithVariantsDetailModel = UmbEntryWithVariantsDetailModel,
>
	extends UmbVariantDatasetWorkspaceContext<T['variants'][0]>, UmbEntryWorkspaceContext {
	// Data:
	readonly valueVariants: UmbEntryDataValueVariantsController<T>;
}
