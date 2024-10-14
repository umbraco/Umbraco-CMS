import { UmbTemporaryFileRepository } from './temporary-file.repository.js';
import {
	TemporaryFileStatus,
	type UmbQueueHandlerCallback,
	type UmbTemporaryFileModel,
	type UmbUploadOptions,
} from './types.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbTemporaryFileManager<
	UploadableItem extends UmbTemporaryFileModel = UmbTemporaryFileModel,
> extends UmbControllerBase {
	readonly #temporaryFileRepository = new UmbTemporaryFileRepository(this._host);

	readonly #queue = new UmbArrayState<UploadableItem>([], (item) => item.temporaryUnique);
	public readonly queue = this.#queue.asObservable();

	async uploadOne(uploadableItem: UploadableItem, options?: UmbUploadOptions<UploadableItem>): Promise<UploadableItem> {
		this.#queue.setValue([]);

		const item: UploadableItem = {
			status: TemporaryFileStatus.WAITING,
			...uploadableItem,
		};

		this.#queue.appendOne(item);
		return (await this.#handleQueue({ ...options, chunkSize: 1 }))[0];
	}

	async upload(
		queueItems: Array<UploadableItem>,
		options?: UmbUploadOptions<UploadableItem>,
	): Promise<Array<UploadableItem>> {
		this.#queue.setValue([]);

		const items = queueItems.map((item): UploadableItem => ({ status: TemporaryFileStatus.WAITING, ...item }));
		this.#queue.append(items);
		return this.#handleQueue({ ...options });
	}

	removeOne(unique: string) {
		this.#queue.removeOne(unique);
	}

	remove(uniques: Array<string>) {
		this.#queue.remove(uniques);
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

	async #handleUpload(item: UploadableItem) {
		if (!item.temporaryUnique) throw new Error(`Unique is missing for item ${item}`);

		const { error } = await this.#temporaryFileRepository.upload(item.temporaryUnique, item.file);

		let status: TemporaryFileStatus;
		if (error) {
			status = TemporaryFileStatus.ERROR;
			this.#queue.updateOne(item.temporaryUnique, { ...item, status });
		} else {
			status = TemporaryFileStatus.SUCCESS;
			this.#queue.updateOne(item.temporaryUnique, { ...item, status });
		}

		return { ...item, status };
	}
}
