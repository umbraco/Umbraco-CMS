import { UmbMediaDetailRepository } from '../media/repository/index.js';
import type { UmbMediaDetailModel, UmbMediaValueModel } from '../media/types.js';
import { UmbFileDropzoneItemStatus } from './constants.js';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from './modals/index.js';
import type {
	UmbUploadableFile,
	UmbUploadableFolder,
	UmbFileDropzoneDroppedItems,
	UmbFileDropzoneProgress,
	UmbUploadableItem,
	UmbAllowedMediaTypesOfExtension,
	UmbAllowedChildrenOfMediaType,
} from './types.js';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

/**
 * Manages the dropzone and uploads folders and files to the server.
 * @function createMediaItems - Upload files and folders to the server and creates the items using corresponding media type.
 * @function createTemporaryFiles - Upload the files as temporary files and returns the data.
 * @observable progress - Emits the number of completed items and total items.
 * @observable progressItems - Emits the items with their current status.
 */
export class UmbDropzoneManager extends UmbControllerBase {
	readonly #host: UmbControllerHost;
	/**
	 * @deprecated Not used anymore; this method will be removed in Umbraco 17.
	 */
	#isFoldersAllowed = true;

	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	#tempFileManager = new UmbTemporaryFileManager(this);

	// The available media types for a file extension.
	readonly #availableMediaTypesOf = new UmbArrayState<UmbAllowedMediaTypesOfExtension>([], (x) => x.extension);

	// The media types that the parent will allow to be created under it.
	readonly #allowedChildrenOf = new UmbArrayState<UmbAllowedChildrenOfMediaType>([], (x) => x.mediaTypeUnique);

	readonly #progress = new UmbObjectState<UmbFileDropzoneProgress>({ total: 0, completed: 0 });
	public readonly progress = this.#progress.asObservable();

	readonly #progressItems = new UmbArrayState<UmbUploadableItem>([], (x) => x.unique);
	public readonly progressItems = this.#progressItems.asObservable();

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#localization = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#host = host;

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	/**
	 * @param isAllowed
	 * @deprecated Not used anymore; this method will be removed in Umbraco 17.
	 */
	public setIsFoldersAllowed(isAllowed: boolean) {
		this.#isFoldersAllowed = isAllowed;
	}

	/**
	 * @deprecated Not used anymore; this method will be removed in Umbraco 17.
	 */
	public getIsFoldersAllowed(): boolean {
		return this.#isFoldersAllowed;
	}

	/** @deprecated Please use `createMediaItems()` instead; this method will be removed in Umbraco 17. */
	public createFilesAsMedia = this.createMediaItems;

	/**
	 * Uploads files and folders to the server and creates the media items with corresponding media type.\
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param {UmbFileDropzoneDroppedItems} items - The files and folders to upload.
	 * @param {string | null} parentUnique - Where the items should be uploaded.
	 * @returns {Array<UmbUploadableItem>} - The items about to be uploaded.
	 */
	public createMediaItems(items: UmbFileDropzoneDroppedItems, parentUnique: string | null = null) {
		const uploadableItems = this.#setupProgress(items, parentUnique);

		if (!uploadableItems.length) return [];

		if (uploadableItems.length === 1) {
			// When there is only one item being uploaded, allow the user to pick the media type, if more than one is allowed.
			this.#createOneMediaItem(uploadableItems[0]);
		} else {
			// When there are multiple items being uploaded, automatically pick the media types for each item. We probably want to allow the user to pick the media type in the future.
			this.#createMediaItems(uploadableItems);
		}

		return uploadableItems;
	}

	/** @deprecated Please use `createTemporaryFiles()` instead; this method will be removed in Umbraco 17. */
	public createFilesAsTemporary = this.createTemporaryFiles;

	/**
	 * Uploads the files as temporary files and returns the data.
	 * @param { File[] } files - The files to upload.
	 * @returns {Promise<Array<UmbUploadableItem>>} - Files as temporary files.
	 */
	public async createTemporaryFiles(files: Array<File>): Promise<Array<UmbUploadableItem>> {
		const uploadableItems = this.#setupProgress({ files, folders: [] }, null) as Array<UmbUploadableFile>;

		const uploadedItems: Array<UmbUploadableItem> = [];

		for (const item of uploadableItems) {
			// Upload as temp file
			const uploaded = await this.#tempFileManager.uploadOne(item.temporaryFile);

			// Update progress
			if (uploaded.status === TemporaryFileStatus.SUCCESS) {
				this.#updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
			} else {
				this.#updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
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

	async #showDialogMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>) {
		const value = await umbOpenModal(this.#host, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } }).catch(
			() => undefined,
		);
		return value?.mediaTypeUnique;
	}

	async #createOneMediaItem(item: UmbUploadableItem) {
		const options = await this.#getMediaTypeOptions(item);
		if (!options.length) {
			this.#notificationContext?.peek('warning', {
				data: {
					message: `${this.#localization.term('media_disallowedFileType')}: ${item.temporaryFile?.file.name}.`,
				},
			});
			return this.#updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
		}

		const mediaTypeUnique = options.length > 1 ? await this.#showDialogMediaTypePicker(options) : options[0].unique;

		if (!mediaTypeUnique) {
			return this.#updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
		}

		if (item.temporaryFile) {
			this.#handleFile(item as UmbUploadableFile, mediaTypeUnique);
		} else if (item.folder) {
			this.#handleFolder(item as UmbUploadableFolder, mediaTypeUnique);
		}
	}

	async #createMediaItems(uploadableItems: Array<UmbUploadableItem>) {
		for (const item of uploadableItems) {
			const options = await this.#getMediaTypeOptions(item);
			if (!options.length) {
				this.#updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
				continue;
			}

			const mediaTypeUnique = options[0].unique;

			if (!mediaTypeUnique) {
				throw new Error('Media type unique is not defined');
			}

			// Handle files and folders differently: a file is uploaded as temp then created as a media item, and a folder is created as a media item directly
			if (item.temporaryFile) {
				this.#handleFile(item as UmbUploadableFile, mediaTypeUnique);
			} else if (item.folder) {
				this.#handleFolder(item as UmbUploadableFolder, mediaTypeUnique);
			}
		}
	}

	async #handleFile(item: UmbUploadableFile, mediaTypeUnique: string) {
		// Upload the file as a temporary file and update progress.
		const temporaryFile = await this.#uploadAsTemporaryFile(item);
		if (temporaryFile.status === TemporaryFileStatus.CANCELLED) {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
			return;
		}
		if (temporaryFile.status !== TemporaryFileStatus.SUCCESS) {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
			return;
		}

		// Create the media item.
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);

		if (data) {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
		}
	}

	async #handleFolder(item: UmbUploadableFolder, mediaTypeUnique: string) {
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);
		if (data) {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else {
			this.#updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
		}
	}

	#uploadAsTemporaryFile(item: UmbUploadableFile) {
		return this.#tempFileManager.uploadOne(item.temporaryFile);
	}

	// Media types
	async #getMediaTypeOptions(item: UmbUploadableItem): Promise<Array<UmbAllowedMediaTypeModel>> {
		// Check the parent which children media types are allowed
		const parent = item.parentUnique ? await this.#mediaDetailRepository.requestByUnique(item.parentUnique) : null;
		const allowedChildren = await this.#getAllowedChildrenOf(parent?.data?.mediaType.unique ?? null, item.parentUnique);

		const extension = item.temporaryFile?.file.name.split('.').pop() ?? null;

		// Check which media types allow the file's extension
		const availableMediaType = await this.#getAvailableMediaTypesOf(extension);

		if (!availableMediaType.length) return [];

		const options = allowedChildren.filter((x) => availableMediaType.find((y) => y.unique === x.unique));
		return options;
	}

	async #getAvailableMediaTypesOf(extension: string | null) {
		// Check if we already have information on this file extension.
		const available = this.#availableMediaTypesOf
			.getValue()
			.find((x) => x.extension === extension)?.availableMediaTypes;
		if (available) return available;

		// Request information on this file extension
		const availableMediaTypes = extension
			? await this.#mediaTypeStructure.requestMediaTypesOf({ fileExtension: extension })
			: await this.#mediaTypeStructure.requestMediaTypesOfFolders();

		this.#availableMediaTypesOf.appendOne({ extension, availableMediaTypes });
		return availableMediaTypes;
	}

	async #getAllowedChildrenOf(mediaTypeUnique: string | null, parentUnique: string | null) {
		//Check if we already got information on this media type.
		const allowed = this.#allowedChildrenOf
			.getValue()
			.find((x) => x.mediaTypeUnique === mediaTypeUnique)?.allowedChildren;
		if (allowed) return allowed;

		// Request information on this media type.
		const { data } = await this.#mediaTypeStructure.requestAllowedChildrenOf(mediaTypeUnique, parentUnique);
		if (!data) throw new Error('Parent media type does not exists');

		this.#allowedChildrenOf.appendOne({ mediaTypeUnique, allowedChildren: data.items });
		return data.items;
	}

	// Scaffold
	async #getItemScaffold(item: UmbUploadableItem, mediaTypeUnique: string): Promise<UmbMediaDetailModel> {
		// TODO: Use a scaffolding feature to ensure consistency. [NL]
		const name = item.temporaryFile ? item.temporaryFile.file.name : (item.folder?.name ?? '');
		const umbracoFile: UmbMediaValueModel = {
			editorAlias: '',
			alias: 'umbracoFile',
			value: { temporaryFileId: item.temporaryFile?.temporaryUnique },
			culture: null,
			segment: null,
		};

		const preset: Partial<UmbMediaDetailModel> = {
			unique: item.unique,
			mediaType: { unique: mediaTypeUnique, collection: null },
			variants: [{ culture: null, segment: null, createDate: null, updateDate: null, name }],
			values: item.temporaryFile ? [umbracoFile] : undefined,
		};
		const { data } = await this.#mediaDetailRepository.createScaffold(preset);
		return data!;
	}

	// Progress handling
	#setupProgress(items: UmbFileDropzoneDroppedItems, parent: string | null) {
		const current = this.#progress.getValue();
		const currentItems = this.#progressItems.getValue();

		const uploadableItems = this.#prepareItemsAsUploadable({ folders: items.folders, files: items.files }, parent);

		this.#progressItems.setValue([...currentItems, ...uploadableItems]);
		this.#progress.setValue({ total: current.total + uploadableItems.length, completed: current.completed });

		return uploadableItems;
	}

	#updateStatus(item: UmbUploadableItem, status: UmbFileDropzoneItemStatus) {
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
				this.#updateStatus(uploadableItem, UmbFileDropzoneItemStatus.CANCELLED);
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
		this.#tempFileManager.destroy();
		super.destroy();
	}
}
