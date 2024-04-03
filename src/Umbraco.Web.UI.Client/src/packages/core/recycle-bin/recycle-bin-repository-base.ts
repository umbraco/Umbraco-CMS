import type { UmbRecycleBinRepository } from './recycle-bin-repository.interface.js';
import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinDataSourceConstructor,
} from './recycle-bin-data-source.interface.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export abstract class UmbRecycleBinRepositoryBase extends UmbRepositoryBase implements UmbRecycleBinRepository {
	#recycleBinSource: UmbRecycleBinDataSource;

	constructor(host: UmbControllerHost, recycleBinSource: UmbRecycleBinDataSourceConstructor) {
		super(host);
		this.#recycleBinSource = new recycleBinSource(this);
	}

	async requestTrash(args: any) {
		const { error } = await this.#recycleBinSource.trash(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Trashed` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	async requestRestore(args: any) {
		const { error } = await this.#recycleBinSource.restore(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Restored` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	async requestEmpty() {
		const { error } = await this.#recycleBinSource.empty();

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Recycle Bin Emptied` } };
			notificationContext.peek('positive', notification);
		}

		return this.#recycleBinSource.empty();
	}
}
