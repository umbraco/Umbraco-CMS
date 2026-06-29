import type { UmbPublishedVariantWithPendingChanges } from '../publishing/index.js';
import { html, state } from '@umbraco-cms/backoffice/external/lit';
import { DocumentVariantStateModel as UmbPublishableVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbContextMinimal, UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Minimal interface that publishing workspace contexts must satisfy
 * for the variant selector to observe pending changes.
 */
export interface UmbPublishingWorkspaceContextWithPendingChanges extends UmbContextMinimal {
	publishedPendingChanges: {
		variantsWithChanges: Observable<Array<UmbPublishedVariantWithPendingChanges>>;
	};
}

/**
 * A variant selector base class that adds publishing-state awareness
 * (pending changes, publish state labels) to the split-view variant selector.
 * Both Document and Element variant selectors extend this.
 * @abstract
 * @class UmbPublishableSplitViewVariantSelectorElement
 * @augments {UmbWorkspaceSplitViewVariantSelectorElement}
 */
export abstract class UmbPublishableSplitViewVariantSelectorElement<
	VariantOptionModelType extends UmbEntityVariantOptionModel<UmbEntityVariantModel>,
> extends UmbWorkspaceSplitViewVariantSelectorElement<VariantOptionModelType> {
	/**
	 * Returns the context token used to consume the publishing workspace context.
	 * Subclasses must implement this to provide their entity-specific context token.
	 * @returns {UmbContextToken<any>} The context token for the publishing workspace context.
	 */
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	protected abstract getPublishingContextToken(): UmbContextToken<any>;

	@state()
	private _variantsWithPendingChanges: Array<UmbPublishedVariantWithPendingChanges> = [];

	#publishingContext?: UmbPublishingWorkspaceContextWithPendingChanges;

	#publishStateLocalizationMap: Record<string, string> = {
		[UmbPublishableVariantState.DRAFT]: 'content_unpublished',
		[UmbPublishableVariantState.PUBLISHED]: 'content_published',
		// TODO: The pending changes state can be removed once the management Api removes this state
		// We only keep it here to make typescript happy
		// We should also make our own state model for this
		[UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES]: 'content_published',
		[UmbPublishableVariantState.NOT_CREATED]: 'content_notCreated',
		[UmbPublishableVariantState.TRASHED]: 'mediaPicker_trashed',
	};

	constructor() {
		super();
		this.consumeContext(this.getPublishingContextToken(), (instance) => {
			this.#publishingContext = instance as UmbPublishingWorkspaceContextWithPendingChanges;
			this.#observePendingChanges();
		});
	}

	#observePendingChanges() {
		this.observe(
			this.#publishingContext?.publishedPendingChanges.variantsWithChanges,
			(variants) => {
				this._variantsWithPendingChanges = variants || [];
			},
			'_observePendingChanges',
		);
	}

	#isPublished(variantOption: VariantOptionModelType) {
		return (
			variantOption.variant?.state === UmbPublishableVariantState.PUBLISHED ||
			variantOption.variant?.state === UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES
		);
	}

	#hasPendingChanges(variant: VariantOptionModelType) {
		return this._variantsWithPendingChanges.some((x) => x.variantId.compare(variant));
	}

	#getVariantState(variantOption: VariantOptionModelType) {
		let term =
			this.#publishStateLocalizationMap[variantOption.variant?.state || UmbPublishableVariantState.NOT_CREATED];

		if (this.#isPublished(variantOption) && this.#hasPendingChanges(variantOption)) {
			term = 'content_publishedPendingChanges';
		}

		return this.localize.term(term);
	}

	protected override _renderVariantDetails(variantOption: VariantOptionModelType) {
		return html`${this.#getVariantState(variantOption)}`;
	}
}
