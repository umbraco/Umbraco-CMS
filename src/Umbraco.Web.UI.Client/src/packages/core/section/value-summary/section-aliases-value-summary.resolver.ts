import { UmbSectionItemRepository } from '../repository/item/section-item.repository.js';
import type { UmbSectionItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbSectionAliasesValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string[], ReadonlyArray<string>>
{
	#repo = new UmbSectionItemRepository(this);

	async resolveValues(values: ReadonlyArray<string[]>): Promise<UmbValueSummaryResolveResult<ReadonlyArray<string>>> {
		const allAliases = [...new Set(values.flat())];
		const { data, asObservable } = await this.#repo.requestItems(allAliases);
		const items = Array.isArray(data) ? data : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string[]>,
		items: ReadonlyArray<UmbSectionItemModel>,
	): ReadonlyArray<ReadonlyArray<string>> {
		const nameByAlias = new Map(items.map((s) => [s.unique, s.name]));
		return values.map((aliases) => aliases.map((alias) => nameByAlias.get(alias) ?? alias));
	}
}

// Named 'api' for ApiLoaderProperty convention
export { UmbSectionAliasesValueSummaryResolver as api };
