import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { UmbElementItemRepository } from '../../../item/repository/index.js';
import type { UmbElementItemModel } from '../../../types.js';

/** Batch-resolves Element Picker value (array of element uniques) to their item models. */
export class UmbElementPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string> | undefined, Array<UmbElementItemModel>>
{
	#repo = new UmbElementItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<Array<string> | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbElementItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => v ?? []))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbElementItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (latest) => this.#map(values, latest as Array<UmbElementItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<Array<string> | undefined>,
		items: ReadonlyArray<UmbElementItemModel>,
	): ReadonlyArray<Array<UmbElementItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			(v ?? []).map((key) => itemByKey.get(key)).filter((item): item is UmbElementItemModel => !!item),
		);
	}
}

export { UmbElementPickerValueSummaryResolver as valueResolver };
