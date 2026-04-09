import { UmbUserGroupItemRepository } from '../repository/item/user-group-item.repository.js';
import type { UmbUserGroupItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryApi } from '@umbraco-cms/backoffice/value-summary';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbUserGroupValueSummaryApi
	extends UmbControllerBase
	implements UmbValueSummaryApi<UmbReferenceByUnique[], ReadonlyArray<UmbUserGroupItemModel>>
{
	#repo = new UmbUserGroupItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<UmbReferenceByUnique[]>,
	): Promise<ReadonlyArray<ReadonlyArray<UmbUserGroupItemModel>>> {
		const allIds = [...new Set(values.flatMap((v) => v.map((r) => r.unique)))];
		const { data } = await this.#repo.requestItems(allIds);
		const items = Array.isArray(data) ? data : [];
		const itemByUnique = new Map(items.map((g) => [g.unique, g]));
		return values.map((v) => v.map((r) => itemByUnique.get(r.unique)).filter(Boolean) as UmbUserGroupItemModel[]);
	}
}

export { UmbUserGroupValueSummaryApi as api };
