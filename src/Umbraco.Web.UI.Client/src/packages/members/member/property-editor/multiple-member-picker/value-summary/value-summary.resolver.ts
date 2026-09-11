import { UmbMemberItemRepository, type UmbMemberItemModel } from '../../../item/repository/index.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

/** Batch-resolves Multiple Member Picker value (array of member uniques) to their item models. */
export class UmbMultipleMemberPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string> | undefined, Array<UmbMemberItemModel>>
{
	readonly #repo = new UmbMemberItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<Array<string> | undefined>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMemberItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => v ?? []))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbMemberItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () =>
						createObservablePart(asObservable()!, (latest) => this.#map(values, latest as Array<UmbMemberItemModel>))
				: undefined,
		};
	}

	#map(
		values: ReadonlyArray<Array<string> | undefined>,
		items: ReadonlyArray<UmbMemberItemModel>,
	): ReadonlyArray<Array<UmbMemberItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			(v ?? []).map((key) => itemByKey.get(key)).filter((item): item is UmbMemberItemModel => !!item),
		);
	}
}
