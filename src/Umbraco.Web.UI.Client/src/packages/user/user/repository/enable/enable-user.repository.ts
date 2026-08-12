import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUserDetailRepository } from '../detail/user-detail.repository.js';
import { UmbEnableUserServerDataSource } from './enable-user.server.data-source.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEnableUserRepository extends UmbUserRepositoryBase {
	#enableSource: UmbEnableUserServerDataSource;
	#detailRepository = new UmbUserDetailRepository(this);
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#enableSource = new UmbEnableUserServerDataSource(host);
	}

	async enable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.init;

		const { error } = await this.#enableSource.enable(ids);
		if (error) {
			return { error };
		}

		// Refresh the enabled users from the server so the store reflects their computed state
		// (an approved user that has never logged in is Inactive, not Active).
		const { data } = await this.#detailRepository.requestByUniques(ids);
		if (!data) throw new Error('Could not load users');

		let message = this.#localize.term('speechBubbles_enableUsersSuccess', data.length);

		if (data.length === 1) {
			const names = data.map((user) => user.name).join(', ');
			message = this.#localize.term('speechBubbles_enableUserSuccess', names);
		}

		const notification = { data: { message } };
		this.notificationContext?.peek('positive', notification);

		return { error };
	}
}

export default UmbEnableUserRepository;
