import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from './types.js';
import { UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

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

	public override async requestCollection() {
		await this.observe(this.#displayCultureObservable)?.asPromise();
		await super.requestCollection();
	}
}

export { UmbDocumentCollectionContext as api };
