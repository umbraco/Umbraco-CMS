import { UmbUserGroupCollectionRepository, UmbUserGroupItemRepository } from '@umbraco-cms/backoffice/user-group';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbUserGroupDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#userGroupCollectionRepository: UmbUserGroupCollectionRepository;
	#userGroupItemRepository: UmbUserGroupItemRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);
		this.#userGroupItemRepository = new UmbUserGroupItemRepository(this);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data, error } = await this.#userGroupCollectionRepository.requestCollection({
			skip: args.skip ?? 0,
			take: args.take ?? 100,
			filter: args.filter,
		});

		if (error || !data) {
			return { data: undefined };
		}

		return {
			data: {
				items: data.items.map((group) => ({
					unique: group.unique,
					entityType: group.entityType,
					name: group.name,
					icon: group.icon ?? undefined,
				})),
				total: data.total,
			},
		};
	}

	async requestItems(uniques: Array<string>): Promise<{
		data?: Array<UmbDatalistItemModel>;
		asObservable?: () => Observable<Array<UmbDatalistItemModel>> | undefined;
	}> {
		const { data, error, asObservable } = await this.#userGroupItemRepository.requestItems(uniques);

		if (error || !data) {
			return { data: undefined };
		}

		const mapItem = (group: (typeof data)[0]): UmbDatalistItemModel => ({
			unique: group.unique,
			entityType: group.entityType,
			name: group.name,
			icon: group.icon ?? undefined,
		});

		return {
			data: data.map(mapItem),
			asObservable: asObservable ? () => asObservable()?.pipe(map((items) => items.map(mapItem))) : undefined,
		};
	}
}

export { UmbUserGroupDatalistDataSource as api };
