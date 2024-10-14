import { UmbUserRepositoryBase } from '../user-repository-base.js';
import { UmbUserAvatarServerDataSource } from './user-avatar.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbUserAvatarRepository extends UmbUserRepositoryBase {
	#temporaryFileRepository: UmbTemporaryFileRepository;
	#avatarSource: UmbUserAvatarServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#avatarSource = new UmbUserAvatarServerDataSource(host);
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(host);
	}

	/**
	 * Uploads an avatar for the user with the given id
	 * @param {string} userUnique
	 * @param {File} file
	 * @returns {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async uploadAvatar(userUnique: string, file: File) {
		if (!userUnique) throw new Error('Id is missing');
		await this.init;

		// upload temp file
		const fileId = UmbId.new();
		await this.#temporaryFileRepository.upload(fileId, file);

		// assign temp file to avatar
		const { error } = await this.#avatarSource.createAvatar(userUnique, fileId);

		if (!error) {
			// TODO: update store + current user
			const localUrl = URL.createObjectURL(file);

			// The server returns 5 different sizes of the avatar, so we need to mimick that here
			this.detailStore?.updateItem(userUnique, { avatarUrls: [localUrl, localUrl, localUrl, localUrl, localUrl] });

			const notification = { data: { message: `Avatar uploaded` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Removes the avatar for the user with the given id
	 * @param {string} userUnique
	 * @returns {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async deleteAvatar(userUnique: string) {
		if (!userUnique) throw new Error('Id is missing');
		await this.init;

		const { error } = await this.#avatarSource.deleteAvatar(userUnique);

		if (!error) {
			this.detailStore?.updateItem(userUnique, { avatarUrls: [] });

			const notification = { data: { message: `Avatar deleted` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	override destroy() {
		super.destroy();
	}
}

export default UmbUserAvatarRepository;
