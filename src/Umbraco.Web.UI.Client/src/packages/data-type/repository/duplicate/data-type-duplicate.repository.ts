import { UmbDataTypeDetailRepository } from '../detail/data-type-detail.repository.js';
import { UmbDataTypeDuplicateServerDataSource } from './data-type-duplicate.server.data-source.js';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDuplicateRepository, UmbDuplicateDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDuplicateDataTypeRepository extends UmbRepositoryBase implements UmbDuplicateRepository {
	#init: Promise<unknown>;
	#duplicateSource: UmbDuplicateDataSource;
	#detailRepository: UmbDataTypeDetailRepository;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#duplicateSource = new UmbDataTypeDuplicateServerDataSource(this);
		this.#detailRepository = new UmbDataTypeDetailRepository(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async duplicate(unique: string, targetUnique: string | null) {
		await this.#init;
		const { data: dataTypeDuplicateUnique, error } = await this.#duplicateSource.duplicate(unique, targetUnique);
		if (error) return { error };

		if (dataTypeDuplicateUnique) {
			const { data: dataTypeDuplicate } = await this.#detailRepository.requestByUnique(dataTypeDuplicateUnique);
			if (!dataTypeDuplicate) throw new Error('Could not find copied data type');
			// TODO: reload tree
			const notification = { data: { message: `Data type copied` } };
			this.#notificationContext!.peek('positive', notification);
		}

		return { data: dataTypeDuplicateUnique };
	}
}
