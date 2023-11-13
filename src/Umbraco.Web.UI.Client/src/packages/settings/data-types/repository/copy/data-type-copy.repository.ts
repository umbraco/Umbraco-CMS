import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { UmbDataTypeCopyServerDataSource } from '../sources/data-type-copy.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCopyDataSource, UmbCopyRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeCopyRepository extends UmbDataTypeRepositoryBase implements UmbCopyRepository {
	#copySource: UmbCopyDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#copySource = new UmbDataTypeCopyServerDataSource(this);
	}

	async copy(id: string, targetId: string | null) {
		await this._init;
		const { data: dataTypeCopyId, error } = await this.#copySource.copy(id, targetId);
		if (error) return { error };

		if (dataTypeCopyId) {
			const { data: dataTypeCopy } = await this.requestById(dataTypeCopyId);
			if (!dataTypeCopy) throw new Error('Could not find copied data type');

			// TODO: Be aware about this responsibility.
			this._treeStore!.appendItems([dataTypeCopy]);
			// only update the target if its not the root
			if (targetId) {
				this._treeStore!.updateItem(targetId, { hasChildren: true });
			}

			const notification = { data: { message: `Data type copied` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { data: dataTypeCopyId };
	}
}
