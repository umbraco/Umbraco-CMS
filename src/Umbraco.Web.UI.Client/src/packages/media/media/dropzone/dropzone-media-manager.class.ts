import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from '../constants.js';
import { UmbMediaDetailRepository } from '../repository/detail/index.js';
import type { UmbMediaDetailModel, UmbMediaValueModel } from '../types.js';

import {
	UmbFileDropzoneItemStatus,
	type UmbDropzoneManager,
	type UmbAllowedChildrenOfMediaType,
	type UmbAllowedMediaTypesOfExtension,
	type UmbUploadableFile,
	type UmbUploadableFolder,
	type UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbMediaTypeStructureRepository, type UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDropzoneMediaManager extends UmbControllerBase {
	// The available media types for a file extension.
	readonly #availableMediaTypesOf = new UmbArrayState<UmbAllowedMediaTypesOfExtension>([], (x) => x.extension);

	// The media types that the parent will allow to be created under it.
	readonly #allowedChildrenOf = new UmbArrayState<UmbAllowedChildrenOfMediaType>([], (x) => x.mediaTypeUnique);

	readonly #mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	readonly #mediaDetailRepository = new UmbMediaDetailRepository(this);
	readonly #localization = new UmbLocalizationController(this);
	readonly #dropzoneManager;

	constructor(host: UmbControllerHost, dropzoneManager: UmbDropzoneManager) {
		super(host);
		this.#dropzoneManager = dropzoneManager;
	}

	/**
	 * Uploads files and folders to the server and creates the media items with corresponding media type.\
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param {Array<UmbUploadableItem>} uploadableItems - The files and folders to upload.
	 * @returns {Array<UmbUploadableItem>} - The items about to be uploaded.
	 */
	public createMediaItems(uploadableItems: Array<UmbUploadableItem>): Array<UmbUploadableItem> {
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

	async #showDialogMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } });
		const value = await modalContext.onSubmit().catch(() => undefined);
		return value?.mediaTypeUnique;
	}

	async #createOneMediaItem(item: UmbUploadableItem) {
		const options = await this.#getMediaTypeOptions(item);
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!options.length) {
			notificationContext?.peek('warning', {
				data: {
					message: `${this.#localization.term('media_disallowedFileType')}: ${item.temporaryFile?.file.name}.`,
				},
			});
			this.#dropzoneManager.updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
			return;
		}

		const mediaTypeUnique = options.length > 1 ? await this.#showDialogMediaTypePicker(options) : options[0].unique;

		if (!mediaTypeUnique) {
			this.#dropzoneManager.updateStatus(item, UmbFileDropzoneItemStatus.CANCELLED);
			return;
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
				this.#dropzoneManager.updateStatus(item, UmbFileDropzoneItemStatus.NOT_ALLOWED);
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
		// Create the media item.
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { error } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);

		this.#dropzoneManager.updateStatus(
			item,
			error ? UmbFileDropzoneItemStatus.ERROR : UmbFileDropzoneItemStatus.COMPLETE,
		);
	}

	async #handleFolder(item: UmbUploadableFolder, mediaTypeUnique: string) {
		const scaffold = await this.#getItemScaffold(item, mediaTypeUnique);
		const { error } = await this.#mediaDetailRepository.create(scaffold, item.parentUnique);

		this.#dropzoneManager.updateStatus(
			item,
			error ? UmbFileDropzoneItemStatus.ERROR : UmbFileDropzoneItemStatus.COMPLETE,
		);
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
}
