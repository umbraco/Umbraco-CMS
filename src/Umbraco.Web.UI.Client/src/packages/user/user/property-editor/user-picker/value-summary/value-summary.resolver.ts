import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbUserItemRepository, type UmbUserItemModel } from '../../../repository/item/index.js';

export class UmbUserPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | undefined, Array<UmbUserItemModel>>
{
	#repo = new UmbUserItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbUserItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => splitStringToArray(v)))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbUserItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbUserItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string | undefined>,
		items: ReadonlyArray<UmbUserItemModel>,
	): ReadonlyArray<Array<UmbUserItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			splitStringToArray(v)
				.map((key) => itemByKey.get(key))
				.filter((item): item is UmbUserItemModel => !!item),
		);
	}
}

export { UmbUserPickerValueSummaryResolver as valueResolver };
