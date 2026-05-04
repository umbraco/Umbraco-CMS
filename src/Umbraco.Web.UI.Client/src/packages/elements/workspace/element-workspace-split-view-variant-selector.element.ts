import type { UmbElementVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { PublishableVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-element-workspace-split-view-variant-selector')
export class UmbElementWorkspaceSplitViewVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement<UmbElementVariantOptionModel> {
	protected override _variantSorter = sortVariants;

	@state()
	private _variantsWithPendingChanges: Array<any> = [];

	#elementPublishingWorkspaceContext?: typeof UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT.TYPE;

	#publishStateLocalizationMap = {
		[PublishableVariantStateModel.DRAFT]: 'content_unpublished',
		[PublishableVariantStateModel.PUBLISHED]: 'content_published',
		// TODO: The pending changes state can be removed once the management Api removes this state
		// We only keep it here to make typescript happy
		// We should also make our own state model for this
		[PublishableVariantStateModel.PUBLISHED_PENDING_CHANGES]: 'content_published',
		[PublishableVariantStateModel.NOT_CREATED]: 'content_notCreated',
		[PublishableVariantStateModel.TRASHED]: 'mediaPicker_trashed',
	};

	constructor() {
		super();
		this.consumeContext(UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT, (instance) => {
			this.#elementPublishingWorkspaceContext = instance;
			this.#observePendingChanges();
		});
	}

	#observePendingChanges() {
		this.observe(
			this.#elementPublishingWorkspaceContext?.publishedPendingChanges.variantsWithChanges,
			(variants) => {
				this._variantsWithPendingChanges = variants || [];
			},
			'_observePendingChanges',
		);
	}

	#hasPendingChanges(variant: UmbElementVariantOptionModel) {
		return this._variantsWithPendingChanges.some((x) => x.variantId.compare(variant));
	}

	#getVariantState(variantOption: UmbElementVariantOptionModel) {
		let term =
			this.#publishStateLocalizationMap[variantOption.variant?.state || PublishableVariantStateModel.NOT_CREATED];

		if (
			(variantOption.variant?.state === PublishableVariantStateModel.PUBLISHED ||
				variantOption.variant?.state === PublishableVariantStateModel.PUBLISHED_PENDING_CHANGES) &&
			this.#hasPendingChanges(variantOption)
		) {
			term = 'content_publishedPendingChanges';
		}

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
