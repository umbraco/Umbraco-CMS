import { UmbUserRepositoryBase } from '../user-repository-base.js';
import type { UmbUserDetailDataSource } from '../../types.js';
import { UmbUserServerDataSource } from './user-detail.server.data-source.js';
import type { IUmbUserDetailRepository } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';
import type { CreateUserRequestModel, UpdateUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbUserDetailRepository extends UmbUserRepositoryBase implements IUmbUserDetailRepository {
	#detailSource: UmbUserDetailDataSource;
	#temporaryFileRepository: UmbTemporaryFileRepository;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailSource = new UmbUserServerDataSource(host);
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(host);
	}

	/**
	 * Creates a new user scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbUserRepository
	 */

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		return this.#detailSource.createScaffold(parentId);
	}

	/**
	 * Requests the user with the given id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbUserRepository
	 */
	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.init;

		const { data, error } = await this.#detailSource.read(id);

		if (data) {
			this.detailStore!.append(data);
		}

		return { data, error, asObservable: () => this.detailStore!.byId(id) };
	}

	/**
	 * Returns an observable for the user with the given id
	 * @param {string} id
	 * @return {Promise<Observable<UserDetailModel>>}
	 * @memberof UmbUserRepository
	 */
	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.init;
		return this.detailStore!.byId(id);
	}

	/**
	 * Creates a new user
	 * @param {CreateUserRequestModel} userRequestData
	 * @return { Promise<UmbDataSourceSuccessResponse, UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async create(userRequestData: CreateUserRequestModel) {
		if (!userRequestData) throw new Error('Data is missing');
		await this.init;

		const { data, error } = await this.#detailSource.create(userRequestData);

		if (data) {
			this.detailStore?.append(data);

			const notification = { data: { message: `User created` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	/**
	 * Saves the user with the given id
	 * @param {string} id
	 * @param {UpdateUserRequestModel} user
	 * @return {Promise<UmbDataSourceSuccessResponse, UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async save(id: string, user: UpdateUserRequestModel) {
		if (!id) throw new Error('User id is missing');
		if (!user) throw new Error('User update data is missing');
		await this.init;

		const { data, error } = await this.#detailSource.update(id, user);

		if (data) {
			this.detailStore?.append(data);
		}

		if (!error) {
			// TODO: how do we localize here?
			// The localize method shouldn't be part of the UmbControllerHost interface
			// this._host.localize?.term('speechBubbles_editUserSaved') ??
			const notification = {
				data: { message: 'User saved' },
			};
			this.notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	/**
	 * Deletes the user with the given id
	 * @param {string} id
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async delete(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.init;

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			this.detailStore?.removeItem(id);

			const notification = { data: { message: `User deleted` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Uploads an avatar for the user with the given id
	 * @param {string} id
	 * @param {File} file
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async uploadAvatar(id: string, file: File) {
		if (!id) throw new Error('Id is missing');
		await this.init;

		// upload temp file
		const fileId = UmbId.new();
		await this.#temporaryFileRepository.upload(fileId, file);

		// assign temp file to avatar
		const { error } = await this.#detailSource.createAvatar(id, fileId);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar uploaded` } };
			this.notificationContext?.peek('positive', notification);
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
		await this.init;

		const { error } = await this.#detailSource.deleteAvatar(id);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar deleted` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
