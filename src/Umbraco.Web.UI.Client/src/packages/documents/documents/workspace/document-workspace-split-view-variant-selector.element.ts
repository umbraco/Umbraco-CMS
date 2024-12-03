import type { UmbDocumentVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';

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
		[DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES]: 'content_publishedPendingChanges',
		[DocumentVariantStateModel.NOT_CREATED]: 'content_notCreated',
	};

	constructor() {
		super();
		this.consumeContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT, (instance) => {
			this.#documentPublishingWorkspaceContext = instance;
			this.#observePendingChanges();
		});
	}

	async #observePendingChanges() {
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

	override _renderVariantDetails(variantOption: UmbDocumentVariantOptionModel) {
		return html`${this.#hasPendingChanges(variantOption) ? html`<uui-tag>Pending changes</uui-tag>` : nothing}
		${this.localize.term(
			this.#publishStateLocalizationMap[variantOption.variant?.state || DocumentVariantStateModel.NOT_CREATED],
		)}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentWorkspaceSplitViewVariantSelectorElement;
	}
}
