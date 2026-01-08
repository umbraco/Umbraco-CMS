import { UmbTemporaryFileRepository } from './temporary-file.repository.js';
import { UmbTemporaryFileConfigRepository } from './config/index.js';
import {
	TemporaryFileStatus,
	type UmbQueueHandlerCallback,
	type UmbTemporaryFileModel,
	type UmbUploadOptions,
} from './types.js';
import { observeMultiple, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { formatBytes } from '@umbraco-cms/backoffice/utils';
import { isCancelError } from '@umbraco-cms/backoffice/resources';

export class UmbTemporaryFileManager<
	UploadableItem extends UmbTemporaryFileModel = UmbTemporaryFileModel,
> extends UmbControllerBase {
	readonly #temporaryFileRepository = new UmbTemporaryFileRepository(this._host);
	readonly #temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this._host);
	readonly #localization = new UmbLocalizationController(this._host);

	readonly #queue = new UmbArrayState<UploadableItem>([], (item) => item.temporaryUnique);
	public readonly queue = this.#queue.asObservable();

	/**
	 * Gets the temporary file configuration.
	 * @returns {Promise<UmbTemporaryFileConfigRepository>} The temporary file configuration.
	 */
	async getConfiguration(): Promise<UmbTemporaryFileConfigRepository> {
		await this.#temporaryFileConfigRepository.initialized;
		return this.#temporaryFileConfigRepository;
	}

	async uploadOne(uploadableItem: UploadableItem, options?: UmbUploadOptions<UploadableItem>): Promise<UploadableItem> {
		this.#queue.setValue([]);

		const item: UploadableItem = {
			...uploadableItem,
			status: TemporaryFileStatus.WAITING,
		};

		this.#queue.appendOne(item);
		return (await this.#handleQueue({ ...options, chunkSize: 1 }))[0];
	}

	async upload(
		queueItems: Array<UploadableItem>,
		options?: UmbUploadOptions<UploadableItem>,
	): Promise<Array<UploadableItem>> {
		this.#queue.setValue([]);

		const items = queueItems.map((item): UploadableItem => ({ ...item, status: TemporaryFileStatus.WAITING }));
		this.#queue.append(items);
		return this.#handleQueue({ ...options });
	}

	removeOne(unique: string) {
		this.#queue.removeOne(unique);
	}

	remove(uniques: Array<string>) {
		this.#queue.remove(uniques);
	}

	removeAll() {
		this.#queue.setValue([]);
	}

	async #handleQueue(options?: UmbUploadOptions<UploadableItem>): Promise<Array<UploadableItem>> {
		const filesCompleted: Array<UploadableItem> = [];
		const queue = this.#queue.getValue();

		if (!queue.length) return filesCompleted;

		const handler: UmbQueueHandlerCallback<UploadableItem> = async (item) => {
			const completedUpload = await this.#handleUpload(item);
			filesCompleted.push(completedUpload);

			if (options?.callback) await options.callback(completedUpload);
		};

		const chunkSize = options?.chunkSize ?? 5;
		const chunks = Math.ceil(queue.length / chunkSize);

		for (let i = 0; i < chunks; i++) {
			const chunk = queue.slice(i * chunkSize, i * chunkSize + chunkSize);
			await Promise.all(chunk.map(handler));
		}

		return filesCompleted;
	}

	async #validateItem(item: UploadableItem): Promise<boolean> {
		const config = await this.getConfiguration();
		let maxFileSize = await this.observe(config.part('maxFileSize')).asPromise();
		if (maxFileSize) {
			// Convert from kilobytes to bytes
			maxFileSize *= 1024;
			if (item.file.size > maxFileSize) {
				const notification = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				notification.peek('warning', {
					data: {
						headline: 'Upload',
						message: `
	${this.#localization.term('media_invalidFileSize')}: ${item.file.name} (${formatBytes(item.file.size)}).

	${this.#localization.term('media_maxFileSize')} ${formatBytes(maxFileSize)}.
						`,
					},
				});
				return false;
			}
		}

		const fileExtension = item.file.name.split('.').pop() ?? '';

		const [allowedExtensions, disallowedExtensions] = await this.observe(
			observeMultiple([
				this.#temporaryFileConfigRepository.part('allowedUploadedFileExtensions'),
				this.#temporaryFileConfigRepository.part('disallowedUploadedFilesExtensions'),
			]),
		).asPromise();

		if (
			(allowedExtensions?.length && !allowedExtensions.includes(fileExtension)) ||
			(disallowedExtensions?.length && disallowedExtensions.includes(fileExtension))
		) {
			const notification = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			notification.peek('warning', {
				data: {
					message: `${this.#localization.term('media_disallowedFileType')}: ${fileExtension}`,
				},
			});
			return false;
		}

		return true;
	}

	async #handleUpload(item: UploadableItem) {
		if (!item.temporaryUnique) throw new Error(`Unique is missing for item ${item}`);

		const isValid = await this.#validateItem(item);
		if (!isValid) {
			this.#queue.updateOne(item.temporaryUnique, {
				...item,
				status: TemporaryFileStatus.ERROR,
			});
			return { ...item, status: TemporaryFileStatus.ERROR };
		}

		const { error } = await this.#temporaryFileRepository.upload(
			item.temporaryUnique,
			item.file,
			(evt) => {
				// Update progress in percent if a callback is provided
				if (item.onProgress) item.onProgress((evt.loaded / evt.total) * 100);
			},
			item.abortController?.signal ?? item.abortSignal,
		);
		let status = TemporaryFileStatus.SUCCESS;
		if (error) {
			status = TemporaryFileStatus.ERROR;
			if (isCancelError(error)) {
				status = TemporaryFileStatus.CANCELLED;
			}
		}

		this.#queue.updateOne(item.temporaryUnique, { ...item, status });
		return { ...item, status };
	}
}
