import type { UmbValueMinimalDisplayApi } from '@umbraco-cms/backoffice/value-minimal-display';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbUserGroupItemRepository } from '../repository/item/user-group-item.repository.js';

export class UmbUserGroupValueMinimalDisplayApi
	extends UmbControllerBase
	implements UmbValueMinimalDisplayApi<UmbReferenceByUnique[], string>
{
	#repo = new UmbUserGroupItemRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	async resolveValues(values: ReadonlyArray<UmbReferenceByUnique[]>): Promise<Map<string, string>> {
		const allIds = [...new Set(values.flatMap((v) => v.map((r) => r.unique)))];
		const { data } = await this.#repo.requestItems(allIds);
		const items = Array.isArray(data) ? data : [];
		const nameById = new Map(items.map((g) => [g.unique, g.name]));
		return new Map(
			values.map((v) => [
				JSON.stringify(v),
				v
					.map((r) => nameById.get(r.unique))
					.filter(Boolean)
					.join(', '),
			]),
		);
	}
}

export { UmbUserGroupValueMinimalDisplayApi as api };
