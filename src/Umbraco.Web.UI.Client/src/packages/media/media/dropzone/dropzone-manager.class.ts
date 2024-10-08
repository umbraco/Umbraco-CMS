import { UmbMediaDetailRepository } from '../repository/index.js';
import type { UmbMediaDetailModel, UmbMediaValueModel } from '../types.js';
import { UmbFileDropzoneItemStatus } from './types.js';
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
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';

/**
 * Manages the dropzone and uploads folders and files to the server.
 * @function createMediaItems - Upload files and folders to the server and creates the items using corresponding media type.
 * @function createTemporaryFiles - Upload the files as temporary files and returns the data.
 * @observable progress - Emits the number of completed items and total items.
 * @observable progressItems - Emits the items with their current status.
 */
export class UmbDropzoneManager extends UmbControllerBase {
	readonly #host: UmbControllerHost;
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

	constructor(host: UmbControllerHost) {
		super(host);
		this.#host = host;
	}

	public setIsFoldersAllowed(isAllowed: boolean) {
		this.#isFoldersAllowed = isAllowed;
	}

	public getIsFoldersAllowed(): boolean {
		return this.#isFoldersAllowed;
	}

	/** @deprecated Please use `createMediaItems()` instead; this method will be removed in Umbraco 17. */
	public createFilesAsMedia = this.createMediaItems;

	/**
	 * Uploads files and folders to the server and creates the media items with corresponding media type.\
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param {UmbFileDropzoneDroppedItems} items - The files and folders to upload
	 * @param {string | null} parentUnique - Where the items should be uploaded
	 */
	public async createMediaItems(items: UmbFileDropzoneDroppedItems, parentUnique: string | null = null) {
		const uploadableItems = await this.#setupProgress(items, parentUnique);
		if (uploadableItems.length === 1) {
			// When there is only one item being uploaded, allow the user to pick the media type, if more than one is allowed.
			await this.#createOneMediaItem(uploadableItems[0]);
		} else {
			// When there are multiple items being uploaded, automatically pick the media types for each item. We probably want to allow the user to pick the media type in the future.
			await this.#createMediaItems(uploadableItems);
		}
	}

	/** @deprecated Please use `createTemporaryFiles()` instead; this method will be removed in Umbraco 17. */
	public createFilesAsTemporary = this.createTemporaryFiles;

	/**
	 * Uploads the files as temporary files and returns the data.
	 * @param { File[] } files - The files to upload.
	 * @returns {Promise<Array<UmbUploadableFileModel>>} - Files as temporary files.
	 */
	public async createTemporaryFiles(files: Array<File>) {
		const uploadableItems = (await this.#setupProgress({ files, folders: [] }, null)) as Array<UmbUploadableFile>;

		const uploadedItems: Array<UmbTemporaryFileModel> = [];

		for (const item of uploadableItems) {
			// Upload as temp file
			const uploaded = await this.#tempFileManager.uploadOne({
				temporaryUnique: item.temporaryFile.temporaryUnique,
				file: item.temporaryFile.file,
			});

			// Update progress
			const progress = this.#progress.getValue();
			this.#progress.update({ completed: progress.completed + 1 });

			if (uploaded.status === TemporaryFileStatus.SUCCESS) {
				this.#progressItems.updateOne(item.unique, { status: UmbFileDropzoneItemStatus.COMPLETE });
			} else {
				this.#progressItems.updateOne(item.unique, { status: UmbFileDropzoneItemStatus.ERROR });
			}

			// Add to return value
			uploadedItems.push(uploaded);
		}

		return uploadedItems;
	}

	async #showDialogMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this.#host, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } });
		const value = await modalContext.onSubmit().catch(() => undefined);
		return value?.mediaTypeUnique;
	}

	async #createOneMediaItem(item: UmbUploadableItem) {
		const options = await this.#getMediaTypeOptions(item);
		if (!options.length) {
			return this.#updateProgress(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
		}

		const mediaTypeUnique = options.length > 1 ? await this.#showDialogMediaTypePicker(options) : options[0].unique;

		if (!mediaTypeUnique) {
			return this.#updateProgress(item, UmbFileDropzoneItemStatus.CANCELLED);
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
				this.#updateProgress(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
				continue;
			}

			const mediaTypeUnique = options[0].unique;

			// Handle files and folders differently: a file is uploaded as temp then created as a media item, and a folder is created as a media item directly
			if (item.temporaryFile) {
				await this.#handleFile(item as UmbUploadableFile, mediaTypeUnique);
			} else if (item.folder) {
				await this.#handleFolder(item as UmbUploadableFolder, mediaTypeUnique);
			}
		}
	}

	async #handleFile(item: UmbUploadableFile, mediaTypeUnique: string) {
		// Upload the file as a temporary file and update progress.
		const temporaryFile = await this.#uploadAsTemporaryFile(item);
		if (temporaryFile.status !== TemporaryFileStatus.SUCCESS) {
			this.#updateProgress(item, UmbFileDropzoneItemStatus.ERROR);
			return;
		}

		// Create the media item.
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);

		if (data) {
			this.#updateProgress(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else {
			this.#updateProgress(item, UmbFileDropzoneItemStatus.ERROR);
		}
	}

	async #handleFolder(item: UmbUploadableFolder, mediaTypeUnique: string) {
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);
		if (data) {
			this.#updateProgress(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else {
			this.#updateProgress(item, UmbFileDropzoneItemStatus.ERROR);
		}
	}

	async #uploadAsTemporaryFile(item: UmbUploadableFile) {
		return await this.#tempFileManager.uploadOne({
			temporaryUnique: item.temporaryFile.temporaryUnique,
			file: item.temporaryFile.file,
		});
	}

	// Media types
	async #getMediaTypeOptions(item: UmbUploadableItem): Promise<Array<UmbAllowedMediaTypeModel>> {
		// Check the parent which children media types are allowed
		const parent = item.parentUnique ? await this.#mediaDetailRepository.requestByUnique(item.parentUnique) : null;
		const allowedChildren = await this.#getAllowedChildrenOf(parent?.data?.mediaType.unique ?? null);

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

	async #getAllowedChildrenOf(mediaTypeUnique: string | null) {
		//Check if we already got information on this media type.
		const allowed = this.#allowedChildrenOf
			.getValue()
			.find((x) => x.mediaTypeUnique === mediaTypeUnique)?.allowedChildren;
		if (allowed) return allowed;

		// Request information on this media type.
		const { data } = await this.#mediaTypeStructure.requestAllowedChildrenOf(mediaTypeUnique);
		if (!data) throw new Error('Parent media type does not exists');

		this.#allowedChildrenOf.appendOne({ mediaTypeUnique, allowedChildren: data.items });
		return data.items;
	}

	// Scaffold
	async #getItemScaffold(item: UmbUploadableItem, mediaTypeUnique: string): Promise<UmbMediaDetailModel> {
		// TODO: Use a scaffolding feature to ensure consistency. [NL]
		const name = item.temporaryFile ? item.temporaryFile.file.name : (item.folder?.name ?? '');
		const umbracoFile: UmbMediaValueModel = {
			editorAlias: null as any,
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
	async #setupProgress(items: UmbFileDropzoneDroppedItems, parent: string | null) {
		const current = this.#progress.getValue();
		const currentItems = this.#progressItems.getValue();

		const uploadableItems = this.#prepareItemsAsUploadable({ folders: items.folders, files: items.files }, parent);

		this.#progressItems.setValue([...currentItems, ...uploadableItems]);
		this.#progress.setValue({ total: current.total + uploadableItems.length, completed: current.completed });

		return uploadableItems;
	}

	#updateProgress(item: UmbUploadableItem, status: UmbFileDropzoneItemStatus) {
		this.#progressItems.updateOne(item.unique, { status });
		const progress = this.#progress.getValue();
		this.#progress.update({ completed: progress.completed + 1 });
	}

	readonly #prepareItemsAsUploadable = (
		{ folders, files }: UmbFileDropzoneDroppedItems,
		parentUnique: string | null,
	): Array<UmbUploadableItem> => {
		const items: Array<UmbUploadableItem> = [];

		for (const file of files) {
			const unique = UmbId.new();
			if (file.type) {
				items.push({
					unique,
					parentUnique,
					status: UmbFileDropzoneItemStatus.WAITING,
					temporaryFile: { file, temporaryUnique: UmbId.new() },
				});
			}
		}

		for (const subfolder of folders) {
			const unique = UmbId.new();
			items.push({
				unique,
				parentUnique,
				status: UmbFileDropzoneItemStatus.WAITING,
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
