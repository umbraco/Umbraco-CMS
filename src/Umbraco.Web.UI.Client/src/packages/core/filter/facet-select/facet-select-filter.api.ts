import { UMB_FILTER_CONTEXT } from '../filter.context-token.js';
import type { ManifestFilter } from '../filter.extension.js';
import type { UmbFacetModel } from '../types.js';
import { UmbSelectFilterApiBase } from '../select/select-filter-api.base.js';
import type { UmbSelectFilterOptionModel } from '../select/types.js';
import type { MetaFilterFacetSelect } from './types.js';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

/**
 * API of the `facetSelect` filter kind: the options are the values of the filter's facet in the faceted result,
 * and the filter's item repository resolves those values and the selected values into items.
 */
export class UmbFacetSelectFilterApi<
	OptionType extends UmbSelectFilterOptionModel = UmbSelectFilterOptionModel,
	ItemType extends UmbItemModel = UmbItemModel,
> extends UmbSelectFilterApiBase<OptionType> {
	#facets: Array<UmbFacetModel> = [];
	#valueUniques: Array<string> = [];
	#facetItemsManager?: UmbRepositoryItemsManager<ItemType>;
	#valueItemsManager?: UmbRepositoryItemsManager<ItemType>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_FILTER_CONTEXT, (context) => {
			this.observe(
				context?.facetedResult,
				(result) => {
					this.#facets = (result as Array<UmbFacetModel> | undefined) ?? [];
					this.#facetItemsManager?.setUniques(this.#facets.map((facet) => facet.unique));
					this.#setOptions();
				},
				'umbFacetedResultObserver',
			);
		});
	}

	protected override _onManifest(manifest: ManifestFilter | undefined) {
		const itemRepositoryAlias = (manifest?.meta as MetaFilterFacetSelect | undefined)?.itemRepositoryAlias;
		if (!itemRepositoryAlias || this.#facetItemsManager) return;

		this.#facetItemsManager = new UmbRepositoryItemsManager<ItemType>(this, itemRepositoryAlias);
		this.#valueItemsManager = new UmbRepositoryItemsManager<ItemType>(this, itemRepositoryAlias);

		this.observe(this.#facetItemsManager.items, () => this.#setOptions(), 'umbFacetItemsObserver');
		this.observe(this.#valueItemsManager.items, (items) => this._setValueItems(items), 'umbValueItemsObserver');

		this.#facetItemsManager.setUniques(this.#facets.map((facet) => facet.unique));
		this.#valueItemsManager.setUniques(this.#valueUniques);
	}

	protected override _requestValueItems(uniques: Array<string>) {
		this.#valueUniques = uniques;
		this.#valueItemsManager?.setUniques(uniques);
	}

	/**
	 * Builds the option of a facet value.
	 * @param {UmbFacetModel} facet - The facet value.
	 * @param {ItemType | undefined} item - The item of the facet value, if it is resolved.
	 * @returns {OptionType} The option.
	 */
	protected _toOption(facet: UmbFacetModel, item: ItemType | undefined): OptionType {
		return { ...facet, name: item?.name, icon: item?.icon } as OptionType;
	}

	#setOptions() {
		const items = this.#facetItemsManager?.getItems() ?? [];
		this._setOptions(
			this.#facets.map((facet) =>
				this._toOption(
					facet,
					items.find((item) => item.unique === facet.unique),
				),
			),
		);
	}
}

export { UmbFacetSelectFilterApi as api };
