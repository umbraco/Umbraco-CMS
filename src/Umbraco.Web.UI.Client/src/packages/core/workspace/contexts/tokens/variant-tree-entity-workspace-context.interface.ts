import type { UmbTreeEntityWorkspaceContext } from './tree-entity-workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContext } from './variant-dataset-workspace-context.interface.js';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbVariantTreeEntityWorkspaceContext<VariantType extends UmbEntityVariantModel = UmbEntityVariantModel>
	extends UmbTreeEntityWorkspaceContext,
		UmbVariantDatasetWorkspaceContext<VariantType> {}
