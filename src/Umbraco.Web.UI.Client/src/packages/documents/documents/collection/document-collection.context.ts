import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from './types.js';
import { UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

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

	/**
	 * Returns the href for a specific Document collection item.
	 * @param {UmbDocumentCollectionItemModel} item - The document item to get the href for.
	 * @returns {Promise<string | undefined>} - The edit workspace href for the document.
	 */
	override async requestItemHref(item: UmbDocumentCollectionItemModel): Promise<string | undefined> {
		return `${UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique })}`;
	}
}

export { UmbDocumentCollectionContext as api };
