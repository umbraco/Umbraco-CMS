import { UmbFileDropzoneItemStatus } from './constants.js';
import type {
	UmbUploadableFile,
	UmbFileDropzoneDroppedItems,
	UmbFileDropzoneProgress,
	UmbUploadableItem,
} from './types.js';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

/**
 * Manages the dropzone and uploads folders and files to the server.
 * @function createMediaItems - Upload files and folders to the server and creates the items using corresponding media type.
 * @function createTemporaryFiles - Upload the files as temporary files and returns the data.
 * @observable progress - Emits the number of completed items and total items.
 * @observable progressItems - Emits the items with their current status.
 */
export class UmbDropzoneManager extends UmbControllerBase {
	protected readonly _tempFileManager = new UmbTemporaryFileManager(this);

	readonly #progress = new UmbObjectState<UmbFileDropzoneProgress>({ total: 0, completed: 0 });
	public readonly progress = this.#progress.asObservable();

	readonly #progressItems = new UmbArrayState<UmbUploadableItem>([], (x) => x.unique);
	public readonly progressItems = this.#progressItems.asObservable();

	/**
	 * Uploads the files as temporary files and returns the data.
	 * @param { File[] } files - The files to upload.
	 * @returns {Promise<Array<UmbUploadableItem>>} - Files as temporary files.
	 */
	public async createTemporaryFiles(files: Array<File>): Promise<Array<UmbUploadableItem>> {
		const uploadableItems = this._setupProgress({ files, folders: [] }, null) as Array<UmbUploadableFile>;

		const uploadedItems: Array<UmbUploadableItem> = [];

		for (const item of uploadableItems) {
			// Upload as temp file
			const uploaded = await this._tempFileManager.uploadOne(item.temporaryFile);

			// Update progress
			if (uploaded.status === TemporaryFileStatus.CANCELLED) {
				this._updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
			} else if (uploaded.status === TemporaryFileStatus.SUCCESS) {
				this._updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
			} else {
				this._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
			}

			// Add to return value
			uploadedItems.push(item);
		}

		return uploadedItems;
	}

	public removeOne(item: UmbUploadableItem) {
		item.temporaryFile?.abortController?.abort();
		this.#progressItems.removeOne(item.unique);
		if (item.temporaryFile) {
			this._tempFileManager.removeOne(item.temporaryFile.temporaryUnique);
		}
	}

	public remove(items: Array<UmbUploadableItem>) {
		const uniques: string[] = [];
		for (const item of items) {
			item.temporaryFile?.abortController?.abort();
			if (item.temporaryFile) {
				uniques.push(item.temporaryFile.temporaryUnique);
			}
		}
		this.#progressItems.remove(uniques);
		const temporaryUniques = items.map((x) => x.temporaryFile?.temporaryUnique).filter((x): x is string => !!x);
		this._tempFileManager.remove(temporaryUniques);
	}

	public removeAll() {
		for (const item of this.#progressItems.getValue()) {
			item.temporaryFile?.abortController?.abort();
		}
		this.#progressItems.setValue([]);
		this._tempFileManager.removeAll();
	}

	// Progress handling
	protected _setupProgress(items: UmbFileDropzoneDroppedItems, parent: string | null) {
		const current = this.#progress.getValue();
		const currentItems = this.#progressItems.getValue();

		const uploadableItems = this.#prepareItemsAsUploadable({ folders: items.folders, files: items.files }, parent);

		this.#progressItems.setValue([...currentItems, ...uploadableItems]);
		this.#progress.setValue({ total: current.total + uploadableItems.length, completed: current.completed });

		return uploadableItems;
	}

	protected _updateStatus(item: UmbUploadableItem, status: UmbFileDropzoneItemStatus) {
		this.#progressItems.updateOne(item.unique, { status });
		const progress = this.#progress.getValue();
		this.#progress.update({ completed: progress.completed + 1 });
	}

	#updateProgress(item: UmbUploadableItem, progress: number) {
		this.#progressItems.updateOne(item.unique, { progress });
	}

	readonly #prepareItemsAsUploadable = (
		{ folders, files }: UmbFileDropzoneDroppedItems,
		parentUnique: string | null,
	): Array<UmbUploadableItem> => {
		const items: Array<UmbUploadableItem> = [];

		for (const file of files) {
			const temporaryFile: UmbTemporaryFileModel = {
				file,
				temporaryUnique: UmbId.new(),
				abortController: new AbortController(),
				onProgress: (progress) => this.#updateProgress(uploadableItem, progress),
			};

			const uploadableItem: UmbUploadableFile = {
				unique: UmbId.new(),
				parentUnique,
				status: UmbFileDropzoneItemStatus.WAITING,
				progress: 0,
				temporaryFile,
			};

			temporaryFile.abortController?.signal.addEventListener('abort', () => {
				this._updateStatus(uploadableItem, UmbFileDropzoneItemStatus.CANCELLED);
			});

			items.push(uploadableItem);
		}

		for (const subfolder of folders) {
			const unique = UmbId.new();
			items.push({
				unique,
				parentUnique,
				status: UmbFileDropzoneItemStatus.WAITING,
				progress: 100, // Folders are created instantly.
				folder: { name: subfolder.folderName },
			});

			items.push(...this.#prepareItemsAsUploadable({ folders: subfolder.folders, files: subfolder.files }, unique));
		}
		return items;
	};

	public override destroy() {
		this._tempFileManager.destroy();
		super.destroy();
	}
}
