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

	async move(id: string, targetId: string | null) {
		await this._init;
		const { error } = await this.#moveSource.move(id, targetId);

		if (!error) {
			// TODO: Be aware about this responsibility.
			this._treeStore!.updateItem(id, { parentId: targetId });
			// only update the target if its not the root
			if (targetId) {
				this._treeStore!.updateItem(targetId, { hasChildren: true });
			}

			const notification = { data: { message: `Data type moved` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { error };
	}
}
