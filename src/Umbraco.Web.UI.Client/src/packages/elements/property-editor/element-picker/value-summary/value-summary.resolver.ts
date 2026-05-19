import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { UmbElementItemRepository } from '../../../item/repository/index.js';
import type { UmbElementItemModel } from '../../../item/repository/types.js';

export class UmbElementPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string> | undefined, Array<string>>
{
	#repo = new UmbElementItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<Array<string> | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<string>>> {
		const allKeys = [...new Set(values.flatMap((v) => v ?? []))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbElementItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbElementItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<Array<string> | undefined>,
		items: ReadonlyArray<UmbElementItemModel>,
	): ReadonlyArray<Array<string>> {
		const nameByKey = new Map(items.map((item) => [item.unique, item.variants[0]?.name ?? '']));
		return values.map((v) => (v ?? []).map((key) => nameByKey.get(key)).filter((n): n is string => !!n));
	}
}

export { UmbElementPickerValueSummaryResolver as valueResolver };
