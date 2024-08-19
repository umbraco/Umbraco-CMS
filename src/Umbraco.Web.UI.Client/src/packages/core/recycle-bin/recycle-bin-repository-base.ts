import type { UmbRecycleBinRepository } from './recycle-bin-repository.interface.js';
import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinDataSourceConstructor,
} from './recycle-bin-data-source.interface.js';
import type {
	UmbRecycleBinOriginalParentRequestArgs,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
} from './types.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Base class for recycle bin repositories.
 * @abstract
 * @class UmbRecycleBinRepositoryBase
 * @augments {UmbRepositoryBase}
 * @implements {UmbRecycleBinRepository}
 */
export abstract class UmbRecycleBinRepositoryBase extends UmbRepositoryBase implements UmbRecycleBinRepository {
	#recycleBinSource: UmbRecycleBinDataSource;

	/**
	 * Creates an instance of UmbRecycleBinRepositoryBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbRecycleBinDataSourceConstructor} recycleBinSource
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	constructor(host: UmbControllerHost, recycleBinSource: UmbRecycleBinDataSourceConstructor) {
		super(host);
		this.#recycleBinSource = new recycleBinSource(this);
	}

	/**
	 * Requests to trash an item.
	 * @param {UmbRecycleBinTrashRequestArgs} args
	 * @returns {*}
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestTrash(args: UmbRecycleBinTrashRequestArgs) {
		const { error } = await this.#recycleBinSource.trash(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Trashed` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Requests to restore an item.
	 * @param {UmbRecycleBinRestoreRequestArgs} args
	 * @returns {*}
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestRestore(args: UmbRecycleBinRestoreRequestArgs) {
		const { error } = await this.#recycleBinSource.restore(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Restored` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Requests to empty the recycle bin.
	 * @returns {*}
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestEmpty() {
		const { error } = await this.#recycleBinSource.empty();

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Recycle Bin Emptied` } };
			notificationContext.peek('positive', notification);
		}

		return this.#recycleBinSource.empty();
	}

	/**
	 * Requests the original parent of an item.
	 * @param {UmbRecycleBinOriginalParentRequestArgs} args
	 * @returns {*}
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		return this.#recycleBinSource.getOriginalParent(args);
	}
}
