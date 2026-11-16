import { UmbMemberItemRepository, type UmbMemberItemModel } from '../../../item/repository/index.js';
import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

/**
 * The value of a member picker property: a comma-separated string of member uniques for a picker holding one
 * member, or an array of member uniques for a picker holding several.
 */
export type UmbMemberPickerValueSummaryValue = string | Array<string> | undefined;

/**
 * Batch-resolves member picker values to the picked members' item models, whichever shape the value takes.
 */
export class UmbMemberPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<UmbMemberPickerValueSummaryValue, Array<UmbMemberItemModel>>
{
	readonly #repo = new UmbMemberItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<UmbMemberPickerValueSummaryValue>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbMemberItemModel>>> {
		const allKeys = [...new Set(values.flatMap((v) => this.#toKeys(v)))];
		if (!allKeys.length) return { data: values.map(() => []) };

		const { data, asObservable } = await this.#repo.requestItems(allKeys);
		const items = Array.isArray(data) ? (data as Array<UmbMemberItemModel>) : [];

		return {
			data: this.#map(values, items),
			asObservable: asObservable
				? () => createObservablePart(asObservable()!, (items) => this.#map(values, items as Array<UmbMemberItemModel>))
				: undefined,
		};
	}

	#toKeys(value: UmbMemberPickerValueSummaryValue): Array<string> {
		return Array.isArray(value) ? value : splitStringToArray(value);
	}

	#map(
		values: ReadonlyArray<UmbMemberPickerValueSummaryValue>,
		items: ReadonlyArray<UmbMemberItemModel>,
	): ReadonlyArray<Array<UmbMemberItemModel>> {
		const itemByKey = new Map(items.map((item) => [item.unique, item]));
		return values.map((v) =>
			this.#toKeys(v)
				.map((key) => itemByKey.get(key))
				.filter((item): item is UmbMemberItemModel => !!item),
		);
	}
}
