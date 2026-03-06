import { UmbMediaDetailRepository } from '../repository/detail/index.js';
import type { UmbMediaDetailModel, UmbMediaValueModel } from '../types.js';
import { UMB_MEDIA_PROPERTY_VALUE_ENTITY_TYPE } from '../entity.js';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from './modals/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDropzoneManager,
	UmbFileDropzoneItemStatus,
	type UmbFileDropzoneDroppedItems,
	type UmbUploadableFile,
	type UmbUploadableFolder,
	type UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';
import { getFileExtension } from '@umbraco-cms/backoffice/utils';
import {
	UmbMediaTypeStructureRepository,
	type UmbAllowedChildrenOfMediaType,
	type UmbAllowedMediaTypeModel,
	type UmbAllowedMediaTypesOfExtension,
} from '@umbraco-cms/backoffice/media-type';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { TemporaryFileStatus } from '@umbraco-cms/backoffice/temporary-file';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';

interface UmbMediaTypeOptionsResult {
	options: Array<UmbAllowedMediaTypeModel>;
	availableMediaTypes: Array<UmbAllowedMediaTypeModel>;
}

export class UmbMediaDropzoneManager extends UmbDropzoneManager {
	// The available media types for a file extension.
	readonly #availableMediaTypesOf = new UmbArrayState<UmbAllowedMediaTypesOfExtension>([], (x) => x.extension);

	// The media types that the parent will allow to be created under it.
	readonly #allowedChildrenOf = new UmbArrayState<UmbAllowedChildrenOfMediaType>([], (x) => x.mediaTypeUnique);

	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#localization = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	/**
	 * Uploads files and folders to the server and creates the media items with corresponding media type.\
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param {UmbFileDropzoneDroppedItems} items - The files and folders to upload.
	 * @param {string | null} parentUnique - Where the items should be uploaded.
	 * @returns {Array<UmbUploadableItem>} - The items about to be uploaded.
	 */
	public createMediaItems(
		items: UmbFileDropzoneDroppedItems,
		parentUnique: string | null = null,
	): Array<UmbUploadableItem> {
		const uploadableItems = this._setupProgress(items, parentUnique);

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

	async #createMediaItems(uploadableItems: Array<UmbUploadableItem>) {
		for (const item of uploadableItems) {
			const { options, availableMediaTypes } = await this.#getMediaTypeOptions(item);
			if (!options.length) {
				const message = this.#getDisallowedMessage(item, availableMediaTypes);
				this._updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED, message);
				continue;
			}

			const { unique: mediaTypeUnique, name: mediaTypeName } = options[0];

			if (!mediaTypeUnique) {
				throw new Error('Media type unique is not defined');
			}

			// Handle files and folders differently: a file is uploaded as temp then created as a media item, and a folder is created as a media item directly
			if (item.temporaryFile) {
				this.#handleFile(item as UmbUploadableFile, mediaTypeUnique, mediaTypeName);
			} else if (item.folder) {
				this.#handleFolder(item as UmbUploadableFolder, mediaTypeUnique, mediaTypeName);
			}
		}
	}

	async #handleFile(item: UmbUploadableFile, mediaTypeUnique: string, mediaTypeName: string) {
		// Upload the file as a temporary file and update progress.
		const temporaryFile = await this._tempFileManager.uploadOne(item.temporaryFile);

		if (temporaryFile.status === TemporaryFileStatus.CANCELLED) {
			this._updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
			return;
		}
		if (temporaryFile.status !== TemporaryFileStatus.SUCCESS) {
			this._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
			return;
		}

		// Create the media item.
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data, error } = await this.#mediaDetailRepository.createSilently(scaffold, item.parentUnique);

		if (data) {
			this._updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else if (UmbApiError.isUmbApiError(error) && error.problemDetails?.status === 400) {
			// Validation error — show as inline friendly message (same pattern as NOT_ALLOWED).
			const message = this.#localization.term('media_uploadValidationFailed', mediaTypeName);
			this._updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED, message);
		} else {
			// Other server error — show ERROR status and a manual toast (auto-toast was suppressed).
			this._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
			if (error) {
				this.#notificationContext?.peek('danger', {
					data: { headline: 'An error occurred', message: error.message },
				});
			}
		}
	}

	async #handleFolder(item: UmbUploadableFolder, mediaTypeUnique: string, mediaTypeName: string) {
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { data, error } = await this.#mediaDetailRepository.createSilently(scaffold, item.parentUnique);

		if (data) {
			this._updateStatus(item, UmbFileDropzoneItemStatus.COMPLETE);
		} else if (UmbApiError.isUmbApiError(error) && error.problemDetails?.status === 400) {
			// Validation error — show as inline friendly message (same pattern as NOT_ALLOWED).
			const message = this.#localization.term('media_uploadValidationFailed', mediaTypeName);
			this._updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED, message);
		} else {
			// Other server error — show ERROR status and a manual toast (auto-toast was suppressed).
			this._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
			if (error) {
				this.#notificationContext?.peek('danger', {
					data: { headline: 'An error occurred', message: error.message },
				});
			}
		}
	}

	#getDisallowedMessage(item: UmbUploadableItem, availableMediaTypes: Array<UmbAllowedMediaTypeModel>): string {
		const extension = item.temporaryFile ? getFileExtension(item.temporaryFile.file.name) : undefined;
		if (!extension) {
			return this.#localization.term('media_disallowedFileType');
		}
		if (availableMediaTypes.length === 0) {
			return this.#localization.term('media_disallowedFileExtension', extension);
		}
		const mediaTypeNames = availableMediaTypes.map((x) => x.name).join(', ');
		if (availableMediaTypes.length === 1) {
			return this.#localization.term('media_disallowedMediaTypeNotAllowedHere', extension, mediaTypeNames);
		}
		return this.#localization.term('media_disallowedMediaTypesNotAllowedHere', extension, mediaTypeNames);
	}

	// Media types
	async #getMediaTypeOptions(item: UmbUploadableItem): Promise<UmbMediaTypeOptionsResult> {
		// Check the parent which children media types are allowed
		const parent = item.parentUnique ? await this.#mediaDetailRepository.requestByUnique(item.parentUnique) : null;
		const allowedChildren = await this.#getAllowedChildrenOf(parent?.data?.mediaType.unique ?? null, item.parentUnique);

		const extension = item.temporaryFile ? getFileExtension(item.temporaryFile.file.name) ?? null : null;

		// Check which media types allow the file's extension
		const availableMediaTypes = await this.#getAvailableMediaTypesOf(extension);

		if (!availableMediaTypes.length) return { options: [], availableMediaTypes: [] };

		const options = allowedChildren.filter((x) => availableMediaTypes.find((y) => y.unique === x.unique));
		return { options, availableMediaTypes };
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
		if (!data) throw new Error('Parent media type does not exist');

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
			entityType: UMB_MEDIA_PROPERTY_VALUE_ENTITY_TYPE,
		};

		const preset: Partial<UmbMediaDetailModel> = {
			unique: item.unique,
			mediaType: { unique: mediaTypeUnique, collection: null },
			variants: [{ culture: null, segment: null, createDate: null, updateDate: null, flags: [], name }],
			values: item.temporaryFile ? [umbracoFile] : undefined,
		};
		const { data } = await this.#mediaDetailRepository.createScaffold(preset);
		return data!;
	}

	async #showDialogMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>) {
		const value = await umbOpenModal(this, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } }).catch(
			() => undefined,
		);
		return value?.mediaTypeUnique;
	}

	async #createOneMediaItem(item: UmbUploadableItem) {
		const { options, availableMediaTypes } = await this.#getMediaTypeOptions(item);
		if (!options.length) {
			const message = this.#getDisallowedMessage(item, availableMediaTypes);
			const itemName = item.temporaryFile?.file.name ?? item.folder?.name;
			this.#notificationContext?.peek('warning', {
				data: {
					message: itemName ? `${message} (${itemName}).` : `${message}.`,
				},
			});
			return this._updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED, message);
		}

		const mediaTypeUnique = options.length > 1 ? await this.#showDialogMediaTypePicker(options) : options[0].unique;

		if (!mediaTypeUnique) {
			return this._updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
		}

		const mediaTypeName = options.find((o) => o.unique === mediaTypeUnique)?.name ?? '';

		if (item.temporaryFile) {
			this.#handleFile(item as UmbUploadableFile, mediaTypeUnique, mediaTypeName);
		} else if (item.folder) {
			this.#handleFolder(item as UmbUploadableFolder, mediaTypeUnique, mediaTypeName);
		}
	}
}
