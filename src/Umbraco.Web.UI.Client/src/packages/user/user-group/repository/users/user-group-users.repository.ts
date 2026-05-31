import { UmbUserGroupUsersServerDataSource } from './user-group-users.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserGroupUsersRepository extends UmbRepositoryBase {
	#source: UmbUserGroupUsersServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#source = new UmbUserGroupUsersServerDataSource(host);
	}

	async requestUsersInGroup(groupId: string, take = 100) {
		return this.#source.getUsersInGroup(groupId, take);
	}

	async addUsersToGroup(groupId: string, userIds: string[]) {
		return this.#source.addUsersToGroup(groupId, userIds);
	}

	async removeUsersFromGroup(groupId: string, userIds: string[]) {
		return this.#source.removeUsersFromGroup(groupId, userIds);
	}
}
