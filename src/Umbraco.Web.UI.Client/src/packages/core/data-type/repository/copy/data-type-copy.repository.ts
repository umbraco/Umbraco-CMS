import { UMB_DATA_TYPE_TREE_STORE_CONTEXT, UmbDataTypeTreeStore } from '../../tree/data-type-tree.store.js';
import { UmbDataTypeDetailRepository } from '../detail/data-type-detail.repository.js';
import { UmbDataTypeCopyServerDataSource } from './data-type-copy.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCopyDataSource, UmbCopyRepository, UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbCopyDataTypeRepository extends UmbRepositoryBase implements UmbCopyRepository {
	#init: Promise<unknown>;
	#copySource: UmbCopyDataSource;
	#detailRepository: UmbDataTypeDetailRepository;
	#treeStore?: UmbDataTypeTreeStore;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#copySource = new UmbDataTypeCopyServerDataSource(this);
		this.#detailRepository = new UmbDataTypeDetailRepository(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_DATA_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async copy(unique: string, targetUnique: string | null) {
		await this.#init;
		const { data: dataTypeCopyUnique, error } = await this.#copySource.copy(unique, targetUnique);
		if (error) return { error };

		if (dataTypeCopyUnique) {
			const { data: dataTypeCopy } = await this.#detailRepository.requestByUnique(dataTypeCopyUnique);
			if (!dataTypeCopy) throw new Error('Could not find copied data type');

			// TODO: Be aware about this responsibility.
			this.#treeStore!.append(dataTypeCopy);

			const notification = { data: { message: `Data type copied` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data: dataTypeCopyUnique };
	}
}
