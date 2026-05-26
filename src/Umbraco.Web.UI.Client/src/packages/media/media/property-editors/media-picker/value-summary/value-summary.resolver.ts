import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import type { UmbMediaPickerValueModel } from '../../types.js';
import type { UmbMediaItemModel } from '../../../types.js';
import { UmbMediaItemRepository } from '../../../repository/index.js';

export class UmbMediaPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<UmbMediaPickerValueModel, Array<UmbMediaItemModel>>
{
	#repo = new UmbMediaItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<UmbMediaPickerValueModel>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMediaItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => (v ?? []).map((entry) => entry.mediaKey)))];

		if (allKeys.length === 0) {
			return { data: values.map(() => []) };
		}

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? data : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<UmbMediaPickerValueModel>,
		items: ReadonlyArray<UmbMediaItemModel>,
	): ReadonlyArray<Array<UmbMediaItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map(
			(v) => (v ?? []).map((entry) => itemByKey.get(entry.mediaKey)).filter(Boolean) as Array<UmbMediaItemModel>,
		);
	}
}
