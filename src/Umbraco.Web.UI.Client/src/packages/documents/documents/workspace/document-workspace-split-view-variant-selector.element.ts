import type { UmbDocumentVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';

const elementName = 'umb-document-workspace-split-view-variant-selector';
@customElement(elementName)
export class UmbDocumentWorkspaceSplitViewVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement<UmbDocumentVariantOptionModel> {
	protected override _variantSorter = sortVariants;

	#publishStateLocalizationMap = {
		[DocumentVariantStateModel.DRAFT]: 'content_unpublished',
		[DocumentVariantStateModel.PUBLISHED]: 'content_published',
		[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 'content_publishedPendingChanges',
		[DocumentVariantStateModel.NOT_CREATED]: 'content_notCreated',
	};

	override _renderVariantDetails(variantOption: UmbDocumentVariantOptionModel) {
		return html` ${this.localize.term(
			this.#publishStateLocalizationMap[variantOption.variant?.state || DocumentVariantStateModel.NOT_CREATED],
		)}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentWorkspaceSplitViewVariantSelectorElement;
	}
}
