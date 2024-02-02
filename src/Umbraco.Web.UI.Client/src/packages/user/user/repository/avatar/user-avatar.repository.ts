import type { UmbUserDetailStore } from '../detail/index.js';
import { UMB_USER_DETAIL_STORE_CONTEXT } from '../detail/index.js';
import { UmbUserAvatarServerDataSource } from './user-avatar.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbUserAvatarRepository extends UmbRepositoryBase {
	#init;
	#notificationContext?: UmbNotificationContext;
	#temporaryFileRepository: UmbTemporaryFileRepository;
	#avatarSource: UmbUserAvatarServerDataSource;
	#detailStore: UmbUserDetailStore;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#avatarSource = new UmbUserAvatarServerDataSource(host);
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_USER_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Uploads an avatar for the user with the given id
	 * @param {string} userId
	 * @param {File} file
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async uploadAvatar(userId: string, file: File) {
		if (!userId) throw new Error('Id is missing');
		await this.#init;

		// upload temp file
		const fileId = UmbId.new();
		await this.#temporaryFileRepository.upload(fileId, file);

		// assign temp file to avatar
		const { error } = await this.#avatarSource.createAvatar(userId, fileId);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar uploaded` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Removes the avatar for the user with the given id
	 * @param {string} id
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async deleteAvatar(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { error } = await this.#avatarSource.deleteAvatar(id);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
