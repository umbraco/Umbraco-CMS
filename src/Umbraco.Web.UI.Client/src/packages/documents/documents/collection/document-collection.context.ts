import { UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionContext extends UmbDefaultCollectionContext<
	UmbDocumentCollectionItemModel,
	UmbDocumentCollectionFilterModel
> {
	#variantContext?: typeof UMB_VARIANT_CONTEXT.TYPE;
	#displayCulture = new UmbStringState(undefined);
	#displayCultureObservable = this.#displayCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS);

		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			this.#variantContext = variantContext;
			this.#observeDisplayCulture();
		});
	}

	#observeDisplayCulture() {
		this.observe(
			this.#variantContext?.displayCulture,
			(displayCulture) => {
				if (!displayCulture) return;
				if (this.#displayCulture.getValue() === displayCulture) return;
				this.#displayCulture.setValue(displayCulture);
				this.setFilter({ orderCulture: displayCulture });
			},
			'umbDocumentCollectionDisplayCultureObserver',
		);
	}

	/**
	 * Requests the collection from the repository.
	 * @returns {Promise<void>}
	 * @deprecated Deprecated since v.17.0.0. Use `loadCollection` instead.
	 * @memberof UmbDocumentCollectionContext
	 */
	public override async requestCollection(): Promise<void> {
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: 'requestCollection',
			solution: 'Use .loadCollection method instead',
		}).warn();

		return this._requestCollection();
	}

	protected override async _requestCollection() {
		await this.observe(this.#displayCultureObservable)?.asPromise();
		await super._requestCollection();
	}
}

export { UmbDocumentCollectionContext as api };
