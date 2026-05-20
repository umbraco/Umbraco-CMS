import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbDocumentItemRepository } from '../../../item/repository/index.js';
import type { UmbDocumentItemModel } from '../../../item/repository/types.js';

export class UmbDocumentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string | undefined, Array<UmbDocumentItemModel>>
{
	#repo = new UmbDocumentItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<string | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbDocumentItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => splitStringToArray(v)))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbDocumentItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbDocumentItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<string | undefined>,
		items: ReadonlyArray<UmbDocumentItemModel>,
	): ReadonlyArray<Array<UmbDocumentItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			splitStringToArray(v)
				.map((key) => itemByKey.get(key))
				.filter((item): item is UmbDocumentItemModel => !!item),
		);
	}
}

export { UmbDocumentPickerValueSummaryResolver as valueResolver };
