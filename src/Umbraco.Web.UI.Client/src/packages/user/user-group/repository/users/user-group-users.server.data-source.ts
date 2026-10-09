import { UserGroupService, UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbUserGroupUsersServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getUsersInGroup(groupId: string, take: number) {
		if (!groupId) throw new Error('Group id is missing');

		const { data, error } = await tryExecute(
			this.#host,
			UserService.getFilterUser({ query: { userGroupIds: [groupId], take } }),
		);

		if (error || !data) return { error };

		return {
			data: {
				uniques: data.items.map((u) => u.id),
				total: data.total,
			},
		};
	}

	async addUsersToGroup(groupId: string, userIds: string[]) {
		if (!groupId) throw new Error('Group id is missing');
		if (!userIds.length) return { data: undefined, error: undefined };

		return tryExecute(
			this.#host,
			UserGroupService.postUserGroupByIdUsers({
				path: { id: groupId },
				body: userIds.map((id) => ({ id })),
			}),
			{ disableNotifications: true },
		);
	}

	async removeUsersFromGroup(groupId: string, userIds: string[]) {
		if (!groupId) throw new Error('Group id is missing');
		if (!userIds.length) return { data: undefined, error: undefined };

		return tryExecute(
			this.#host,
			UserGroupService.deleteUserGroupByIdUsers({
				path: { id: groupId },
				body: userIds.map((id) => ({ id })),
			}),
			{ disableNotifications: true },
		);
	}
}
