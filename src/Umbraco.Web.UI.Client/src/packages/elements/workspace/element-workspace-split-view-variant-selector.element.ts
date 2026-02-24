import type { UmbElementVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-element-workspace-split-view-variant-selector')
export class UmbElementWorkspaceSplitViewVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement<UmbElementVariantOptionModel> {
	protected override _variantSorter = sortVariants;

	#publishStateLocalizationMap = {
		[DocumentVariantStateModel.DRAFT]: 'content_unpublished',
		[DocumentVariantStateModel.PUBLISHED]: 'content_published',
		// TODO: The pending changes state can be removed once the management Api removes this state
		// We only keep it here to make typescript happy
		// We should also make our own state model for this
		[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 'content_published',
		[DocumentVariantStateModel.NOT_CREATED]: 'content_notCreated',
		[DocumentVariantStateModel.TRASHED]: 'mediaPicker_trashed',
	};

	#getVariantState(variantOption: UmbElementVariantOptionModel) {
		const term =
			this.#publishStateLocalizationMap[variantOption.variant?.state || DocumentVariantStateModel.NOT_CREATED];
		return this.localize.term(term);
	}

	protected override _renderVariantDetails(variantOption: UmbElementVariantOptionModel) {
		return html`${this.#getVariantState(variantOption)}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-split-view-variant-selector': UmbElementWorkspaceSplitViewVariantSelectorElement;
	}
}
