import { UmbExtensionItemRepository } from '@umbraco-cms/backoffice/extension';
import type { UmbExtensionItemModel } from '@umbraco-cms/backoffice/extension';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbSectionAliasesValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string[], ReadonlyArray<UmbExtensionItemModel>>
{
	#repo = new UmbExtensionItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string[]>,
	): Promise<UmbValueSummaryResolveResult<ReadonlyArray<UmbExtensionItemModel>>> {
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
		items: ReadonlyArray<UmbExtensionItemModel>,
	): ReadonlyArray<ReadonlyArray<UmbExtensionItemModel>> {
		const itemByAlias = new Map(items.map((s) => [s.unique, s]));
		return values.map(
			(aliases) => aliases.map((alias) => itemByAlias.get(alias)).filter(Boolean) as UmbExtensionItemModel[],
		);
	}
}

