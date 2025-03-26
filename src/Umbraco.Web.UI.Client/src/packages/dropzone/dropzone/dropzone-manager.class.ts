import { UmbFileDropzoneItemStatus } from './constants.js';
import type {
	UmbFileDropzoneDroppedItems,
	UmbFileDropzoneProgress,
	UmbUploadableFile,
	UmbUploadableFolder,
	UmbUploadableItem,
} from './types.js';

import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';

/**
 * Manages the dropzone and uploads folders and files to the server.
 * @function createMediaItems - Upload files and folders to the server and creates the items using corresponding media type.
 * @function createTemporaryFiles - Upload the files as temporary files and returns the data.
 * @property {UmbObjectState<UmbFileDropzoneProgress>} progress - Emits the number of completed items and total items.
 * @property {UmbArrayState<UmbUploadableItem>} progressItems - Emits the items with their current status.
 */
export class UmbDropzoneManager extends UmbControllerBase {
	readonly #progress = new UmbObjectState<UmbFileDropzoneProgress>({ total: 0, completed: 0 });
	public readonly progress = this.#progress.asObservable();

	readonly #progressItems = new UmbArrayState<UmbUploadableItem>([], (x) => x.unique);
	public readonly progressItems = this.#progressItems.asObservable();

	#isFoldersAllowed = true;
	#tempFileManager = new UmbTemporaryFileManager(this);

	public setIsFoldersAllowed(isAllowed: boolean) {
		this.#isFoldersAllowed = isAllowed;
	}

	public getIsFoldersAllowed(): boolean {
		return this.#isFoldersAllowed;
	}

	/**
	 * Uploads the files as temporary files and returns the data.
	 * @param {UmbFileDropzoneDroppedItems} items - The items to upload.
	 * @param {string | null} parentUnique - The parent unique.
	 * @returns {Promise<Array<UmbUploadableItem>>} - Files as temporary files.
	 */
	public async createTemporaryFiles(
		items: UmbFileDropzoneDroppedItems,
		parentUnique?: string | null,
	): Promise<Array<UmbUploadableItem>> {
		const uploadableItems = this.#setupProgress(items, parentUnique ?? null);

		const uploadedItems: Array<UmbUploadableItem> = [];

		for (const item of uploadableItems) {
			// Check if the item is a file
			if (this.#isUploadableFile(item)) {
				// Upload as temp file
				const uploaded = await this.#tempFileManager.uploadOne(item.temporaryFile);

				// Update progress
				if (uploaded.status === TemporaryFileStatus.SUCCESS) {
					this._updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
				} else {
					this._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
				}
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
			this.#tempFileManager.removeOne(item.temporaryFile.temporaryUnique);
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
		this.#tempFileManager.remove(temporaryUniques);
	}

	public removeAll() {
		for (const item of this.#progressItems.getValue()) {
			item.temporaryFile?.abortController?.abort();
		}
		this.#progressItems.setValue([]);
		this.#tempFileManager.removeAll();
	}

	// Progress handling
	#setupProgress(items: UmbFileDropzoneDroppedItems, parent: string | null) {
		const current = this.#progress.getValue();

		const uploadableItems = this.#prepareItemsAsUploadable(items, parent);

		this.#progressItems.append(uploadableItems);
		console.log(
			'trying to append',
			uploadableItems,
			'to',
			this.#progressItems,
			'which now has the value of',
			this.#progressItems.getValue(),
		);
		this.#progress.update({
			total: current.total + uploadableItems.length,
		});

		return uploadableItems;
	}

	protected _updateStatus(item: UmbUploadableItem, status: UmbFileDropzoneItemStatus) {
		this.#progressItems.updateOne(item.unique, { status });
		const progress = this.#progress.getValue();
		this.#progress.update({ completed: progress.completed + 1 });
	}

	#isUploadableFile(item: UmbUploadableItem): item is UmbUploadableFile {
		return 'temporaryFile' in item && item.temporaryFile !== undefined;
	}

	#updateProgress(item: UmbUploadableItem, progress: number) {
		this.#progressItems.updateOne(item.unique, { progress });
	}

	#prepareItemsAsUploadable = (
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

		if (!this.getIsFoldersAllowed()) {
			return items;
		}

		for (const subfolder of folders) {
			const unique = UmbId.new();
			items.push({
				unique,
				parentUnique,
				status: UmbFileDropzoneItemStatus.WAITING,
				progress: 100, // Folders are created instantly.
				folder: { name: subfolder.folderName },
			} satisfies UmbUploadableFolder);

			items.push(...this.#prepareItemsAsUploadable({ folders: subfolder.folders, files: subfolder.files }, unique));
		}
		return items;
	};

	public override destroy() {
		this.#tempFileManager.destroy();
		super.destroy();
	}
}
