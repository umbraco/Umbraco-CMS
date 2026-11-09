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
import type { UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbRepositoryErrorResponse, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Base class for recycle bin repositories.
 * @abstract
 * @class UmbRecycleBinRepositoryBase
 * @augments {UmbRepositoryBase}
 * @implements {UmbRecycleBinRepository}
 */
export abstract class UmbRecycleBinRepositoryBase extends UmbRepositoryBase implements UmbRecycleBinRepository {
	#recycleBinSource: UmbRecycleBinDataSource;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#requestRestoreSuccessNotification?: UmbNotificationHandler;

	/**
	 * Creates an instance of UmbRecycleBinRepositoryBase.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbRecycleBinDataSourceConstructor} recycleBinSource - The data source constructor for the recycle bin
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	constructor(host: UmbControllerHost, recycleBinSource: UmbRecycleBinDataSourceConstructor) {
		super(host);
		this.#recycleBinSource = new recycleBinSource(this);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	/**
	 * Requests to trash an item.
	 * @param {UmbRecycleBinTrashRequestArgs} args - The item to trash
	 * @returns {Promise<UmbRepositoryErrorResponse>} The result of the trash request
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestTrash(args: UmbRecycleBinTrashRequestArgs): Promise<UmbRepositoryErrorResponse> {
		return this.#recycleBinSource.trash(args);
	}

	/**
	 * Requests to restore an item.
	 * @param {UmbRecycleBinRestoreRequestArgs} args - The item to restore
	 * @returns {Promise<UmbRepositoryErrorResponse>} The result of the restore request
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestRestore(args: UmbRecycleBinRestoreRequestArgs): Promise<UmbRepositoryErrorResponse> {
		const { error } = await this.#recycleBinSource.restore(args);

		if (!error) {
			// TODO: keep this notification until we refresh the tree/structure correctly after the action
			this.#requestRestoreSuccessNotification?.close();
			const notification = { data: { message: `Restored` } };
			this.#requestRestoreSuccessNotification = this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Requests to empty the recycle bin.
	 * @returns {Promise<UmbRepositoryErrorResponse>} The result of the empty request
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestEmpty(): Promise<UmbRepositoryErrorResponse> {
		return this.#recycleBinSource.empty();
	}

	/**
	 * Requests the original parent of an item.
	 * @param {UmbRecycleBinOriginalParentRequestArgs} args - The item to find the original parent for
	 * @returns {Promise<UmbRepositoryResponse<{ unique: string } | null>>} The original parent of the item
	 * @memberof UmbRecycleBinRepositoryBase
	 */
	async requestOriginalParent(
		args: UmbRecycleBinOriginalParentRequestArgs,
	): Promise<UmbRepositoryResponse<{ unique: string } | null>> {
		return this.#recycleBinSource.getOriginalParent(args) as Promise<UmbRepositoryResponse<{ unique: string } | null>>;
	}
}
