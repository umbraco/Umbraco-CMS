import type { UmbPublishedVariantWithPendingChanges } from '../publishing/index.js';
import { html, state } from '@umbraco-cms/backoffice/external/lit';
import { sortVariants, UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbPublishableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Minimal interface that publishable workspace contexts must satisfy
 * for the variant selector to observe pending changes.
 * @deprecated Deprecated since v17. Publish state is now reported generically via a workspace context's
 * `entityState` registry (`@umbraco-cms/backoffice/entity-state`), rendered by `<umb-entity-state-tags>`
 * in the shared `<umb-workspace-split-view-variant-selector>` — no publishable-specific selector subclass
 * is needed any more. Scheduled for removal in Umbraco 20.
 */
export interface UmbPublishableWorkspaceContextWithPendingChanges extends UmbPublishableWorkspaceContext {
	/** Observable state for detecting pending (unpublished) changes per variant. */
	publishedPendingChanges: {
		variantsWithChanges: Observable<Array<UmbPublishedVariantWithPendingChanges>>;
	};
}

/**
 * A variant selector base class that adds publishable-state awareness
 * (pending changes, publish state labels) to the split-view variant selector.
 * @deprecated Deprecated since v17. Publish state is now reported generically via a workspace context's
 * `entityState` registry (`@umbraco-cms/backoffice/entity-state`), rendered by `<umb-entity-state-tags>`
 * in the shared `<umb-workspace-split-view-variant-selector>` — no publishable-specific selector subclass
 * is needed any more. Scheduled for removal in Umbraco 20.
 * @abstract
 * @class UmbPublishableSplitViewVariantSelectorElement
 * @augments {UmbWorkspaceSplitViewVariantSelectorElement}
 */
export abstract class UmbPublishableSplitViewVariantSelectorElement<
	VariantOptionModelType extends UmbEntityVariantOptionModel<UmbEntityVariantModel>,
	TContext extends UmbPublishableWorkspaceContextWithPendingChanges = UmbPublishableWorkspaceContextWithPendingChanges,
> extends UmbWorkspaceSplitViewVariantSelectorElement<VariantOptionModelType> {
	/**
	 * Returns the context token used to consume the publishable workspace context.
	 * Subclasses must implement this to provide their entity-specific context token.
	 * @returns {UmbContextToken<TContext>} The context token for the publishable workspace context.
	 */
	protected abstract getPublishingContextToken(): UmbContextToken<TContext>;

	public override variantSorter = sortVariants;

	@state()
	private _variantsWithPendingChanges: Array<UmbPublishedVariantWithPendingChanges> = [];

	#publishingContext?: TContext;

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

		new UmbDeprecation({
			deprecated: 'UmbPublishableSplitViewVariantSelectorElement',
			removeInVersion: '20.0.0',
			solution:
				"Publish state is now reported generically via a workspace context's `entityState` registry, rendered by <umb-entity-state-tags> in the shared <umb-workspace-split-view-variant-selector> — no publishable-specific selector subclass is needed any more.",
		}).warn();

		this.consumeContext(this.getPublishingContextToken(), (instance) => {
			this.#publishingContext = instance;
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
