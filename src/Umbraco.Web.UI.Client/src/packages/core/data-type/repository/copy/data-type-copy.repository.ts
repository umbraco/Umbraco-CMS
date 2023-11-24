import { UmbDataTypeRepositoryBase } from '../data-type-repository-base.js';
import { UmbDataTypeDetailRepository } from '../detail/data-type-detail.repository.js';
import { UmbDataTypeCopyServerDataSource } from './data-type-copy.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCopyDataSource, UmbCopyRepository } from '@umbraco-cms/backoffice/repository';

export class UmbCopyDataTypeRepository extends UmbDataTypeRepositoryBase implements UmbCopyRepository {
	#copySource: UmbCopyDataSource;
	#detailRepository: UmbDataTypeDetailRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#copySource = new UmbDataTypeCopyServerDataSource(this);
		this.#detailRepository = new UmbDataTypeDetailRepository(this);
	}

	async copy(unique: string, targetUnique: string | null) {
		await this._init;
		const { data: dataTypeCopyUnique, error } = await this.#copySource.copy(unique, targetUnique);
		if (error) return { error };

		if (dataTypeCopyUnique) {
			const { data: dataTypeCopy } = await this.#detailRepository.requestByUnique(dataTypeCopyUnique);
			if (!dataTypeCopy) throw new Error('Could not find copied data type');

			// TODO: Be aware about this responsibility.
			this._treeStore!.append(dataTypeCopy);

			const notification = { data: { message: `Data type copied` } };
			this._notificationContext!.peek('positive', notification);
		}

		return { data: dataTypeCopyUnique };
	}
}
