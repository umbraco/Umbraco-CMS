import { UmbUserGroupItemRepository } from '../repository/item/user-group-item.repository.js';
import type { UmbValueSummaryApi } from '@umbraco-cms/backoffice/value-summary';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbUserGroupValueSummaryApi
	extends UmbControllerBase
	implements UmbValueSummaryApi<UmbReferenceByUnique[], string>
{
	#repo = new UmbUserGroupItemRepository(this);

	async resolveValues(values: ReadonlyArray<UmbReferenceByUnique[]>): Promise<ReadonlyArray<string>> {
		const allIds = [...new Set(values.flatMap((v) => v.map((r) => r.unique)))];
		const { data } = await this.#repo.requestItems(allIds);
		const items = Array.isArray(data) ? data : [];
		const nameById = new Map(items.map((g) => [g.unique, g.name]));
		return values.map((v) =>
			v
				.map((r) => nameById.get(r.unique))
				.filter(Boolean)
				.join(', '),
		);
	}
}

export { UmbUserGroupValueSummaryApi as api };
