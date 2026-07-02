import type { UmbElementVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbPublishableSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/content';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

@customElement('umb-element-workspace-split-view-variant-selector')
export class UmbElementWorkspaceSplitViewVariantSelectorElement extends UmbPublishableSplitViewVariantSelectorElement<
	UmbElementVariantOptionModel,
	typeof UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT.TYPE
> {
	// UmbElementVariantOptionModel is an empty interface, so TypeScript widens the inherited
	// _variantSorter parameter to the base UmbEntityVariantOptionModel constraint. Declare the
	// override with that signature and delegate to the element-specific sorter.
	protected override _variantSorter = (
		a: UmbEntityVariantOptionModel<UmbEntityVariantModel>,
		b: UmbEntityVariantOptionModel<UmbEntityVariantModel>,
	): number => sortVariants(a as UmbElementVariantOptionModel, b as UmbElementVariantOptionModel);

	protected override getPublishingContextToken() {
		return UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-split-view-variant-selector': UmbElementWorkspaceSplitViewVariantSelectorElement;
	}
}
