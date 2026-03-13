import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { MemberGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '@umbraco-cms/backoffice/member-group';

export class UmbMemberGroupDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data } = await tryExecute(
			this,
			MemberGroupService.getMemberGroup({
				query: {
					skip: args.skip ?? 0,
					take: args.take ?? 100,
				},
			}),
		);

		if (!data) return { data: undefined };

		const items: Array<UmbDatalistItemModel> = data.items.map((item) => ({
			unique: item.name,
			name: item.name,
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
		}));

		return {
			data: {
				items,
				total: data.total,
			},
		};
	}

	async requestItems(uniques: Array<string>): Promise<{ data?: Array<UmbDatalistItemModel> }> {
		return {
			data: uniques.map((name) => ({
				unique: name,
				name: name,
				entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			})),
		};
	}
}
