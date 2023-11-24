import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { UmbDataTypeMoveServerDataSource } from './data-type-move.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMoveDataSource, UmbMoveRepository } from '@umbraco-cms/backoffice/repository';

export class UmbMoveDataTypeRepository extends UmbDataTypeRepositoryBase implements UmbMoveRepository {
	#moveSource: UmbMoveDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#moveSource = new UmbDataTypeMoveServerDataSource(this);
	}

	async move(unique: string, targetUnique: string | null) {
		await this._init;
		const { error } = await this.#moveSource.move(unique, targetUnique);

		if (!error) {
			// TODO: Be aware about this responsibility.
			this._treeStore!.updateItem(unique, { parentUnique: targetUnique });

			const notification = { data: { message: `Data type moved` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
