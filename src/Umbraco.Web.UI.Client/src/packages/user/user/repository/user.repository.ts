import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data-source.js';
import { UmbUserRepositoryBase } from './user-repository-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserRepository extends UmbUserRepositoryBase {
	#setUserGroupsSource: UmbUserSetGroupsServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#setUserGroupsSource = new UmbUserSetGroupsServerDataSource(host);
	}

	async setUserGroups(userIds: Array<string>, userGroupIds: Array<string>) {
		if (userGroupIds.length === 0) throw new Error('User group ids are missing');
		if (userIds.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#setUserGroupsSource.setGroups(userIds, userGroupIds);

		if (!error) {
			//TODO: Update relevant stores
		}

		return { error };
	}
}
