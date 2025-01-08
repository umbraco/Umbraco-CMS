import type { UmbDocumentVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';

const elementName = 'umb-document-workspace-split-view-variant-selector';
@customElement(elementName)
export class UmbDocumentWorkspaceSplitViewVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement<UmbDocumentVariantOptionModel> {
	protected override _variantSorter = sortVariants;

	@state()
	private _variantsWithPendingChanges: Array<any> = [];

	#documentPublishingWorkspaceContext?: typeof UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT.TYPE;

	#publishStateLocalizationMap = {
		[DocumentVariantStateModel.DRAFT]: 'content_unpublished',
		[DocumentVariantStateModel.PUBLISHED]: 'content_published',
		// TODO: The pending changes state can be removed once the management Api removes this state
		// We only keep it here to make typescript happy
		// We should also make our own state model for this
		[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 'content_published',
		[DocumentVariantStateModel.NOT_CREATED]: 'content_notCreated',
	};

	constructor() {
		super();
		this.consumeContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT, (instance) => {
			this.#documentPublishingWorkspaceContext = instance;
			this.#observePendingChanges();
		});
	}

	#observePendingChanges() {
		this.observe(
			this.#documentPublishingWorkspaceContext?.publishedPendingChanges.variantsWithChanges,
			(variants) => {
				this._variantsWithPendingChanges = variants || [];
			},
			'_observePendingChanges',
		);
	}

	#hasPendingChanges(variant: UmbDocumentVariantOptionModel) {
		return this._variantsWithPendingChanges.some((x) => x.variantId.compare(variant));
	}

	#getVariantState(variantOption: UmbDocumentVariantOptionModel) {
		let term = this.#publishStateLocalizationMap[variantOption.variant?.state || DocumentVariantStateModel.NOT_CREATED];

		if (
			(variantOption.variant?.state === DocumentVariantStateModel.PUBLISHED ||
				variantOption.variant?.state === DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES) &&
			this.#hasPendingChanges(variantOption)
		) {
			term = 'content_publishedPendingChanges';
		}

		return this.localize.term(term);
	}

	override _renderVariantDetails(variantOption: UmbDocumentVariantOptionModel) {
		return html` ${this.#getVariantState(variantOption)}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentWorkspaceSplitViewVariantSelectorElement;
	}
}
