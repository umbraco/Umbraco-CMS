import { UmbMediaDetailRepository } from '../repository/index.js';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from './modals/dropzone-media-type-picker/dropzone-media-type-picker-modal.token.js';
import { mimeToExtension } from '@umbraco-cms/backoffice/external/mime-types';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	type UmbAllowedMediaTypeModel,
	UmbMediaTypeDetailRepository,
	UmbMediaTypeStructureRepository,
} from '@umbraco-cms/backoffice/media-type';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

interface UmbUploadableFileModel {
	unique: string;
	file: File;
	mediaTypeUnique: string;
}

export interface UmbUploadableFileExtensionModel {
	fileExtension: string;
	mediaTypes: Array<UmbAllowedMediaTypeModel>;
}

export function getExtensionFromMime(mimeType: string): string | undefined {
	const extension = mimeToExtension(mimeType);
	if (!extension) return; // extension doesn't exist.
	return extension;
}

export class UmbDropzoneManager extends UmbControllerBase {
	#host;
	#tempFileManager = new UmbTemporaryFileManager(this);

	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);
	#mediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#host = host;
	}

	public async dropOneFile(file: File, parentUnique: string | null) {
		const extension = this.#getExtensionFromMimeType(file.type);

		if (!extension) {
			// TODO Folders have no extension on file drop. Assume it is a folder being uploaded.
			return;
		}

		const optionsArray = await this.#buildOptionsArrayFrom([extension], parentUnique);
		if (!optionsArray.length) throw new Error('File not allowed here.'); // Parent does not allow this file type here.

		const mediaTypes = optionsArray[0].mediaTypes;
		if (mediaTypes.length === 1) {
			// Only one allowed option, upload file using that option.
			const uploadableFile: UmbUploadableFileModel = {
				unique: UmbId.new(),
				file,
				mediaTypeUnique: mediaTypes[0].unique,
			};

			await this.#handleUpload([uploadableFile]);
			return;
		}

		// Multiple options, show a dialog for the user to pick one.
		const mediaType = await this.#showDialogMediaTypePicker(mediaTypes);
		if (!mediaType) return; // Upload cancelled.

		const uploadableFile: UmbUploadableFileModel = {
			unique: UmbId.new(),
			file,
			mediaTypeUnique: mediaType.unique,
		};
		await this.#handleUpload([uploadableFile]);
	}

	public async dropFiles(files: Array<File>, parentUnique: string | null) {
		// removes duplicate file types so we don't call endpoints unnecessarily when building options.
		const mimeTypes = [...new Set(files.map<string>((file) => file.type))];
		const optionsArray = await this.#buildOptionsArrayFrom(
			mimeTypes.map((mimetype) => this.#getExtensionFromMimeType(mimetype)),
			parentUnique,
		);

		if (!optionsArray.length) return; // None of the files are allowed in current dropzone.

		// Building an array of uploadable files. Do we want to build an array of failed files to let the user know which ones?
		const uploadableFiles: Array<UmbUploadableFileModel> = [];

		for (const file of files) {
			const extension = this.#getExtensionFromMimeType(file.type);
			if (!extension) {
				// Folders have no extension on file drop. We assume it is a folder being uploaded.
				return;
			}
			const options = optionsArray.find((option) => option.fileExtension === extension)?.mediaTypes;

			if (!options) return; // Dropped file not allowed in current dropzone.

			// Since we are uploading multiple files, we will pick first allowed option.
			// Consider a way we can handle this differently in the future to let the user choose. Maybe a list of all files with an allowed media type dropdown?
			const mediaType = options[0];
			uploadableFiles.push({ unique: UmbId.new(), file, mediaTypeUnique: mediaType.unique });
		}

		await this.#handleUpload(uploadableFiles);
	}

	#getExtensionFromMimeType(mimeType: string): string {
		return getExtensionFromMime(mimeType) || '';
	}

	async #buildOptionsArrayFrom(
		fileExtensions: Array<string>,
		parentUnique: string | null,
	): Promise<Array<UmbUploadableFileExtensionModel>> {
		// Getting all media types allowed in our current position based on parent unique.
		const { data: allAllowedMediaTypes } = await this.#mediaTypeStructure.requestAllowedChildrenOf(parentUnique);
		if (!allAllowedMediaTypes?.items.length) return [];

		const allowedByParent = allAllowedMediaTypes.items;

		// Building an array of options the files can be uploaded as.
		const options: Array<UmbUploadableFileExtensionModel> = [];

		for (const fileExtension of fileExtensions) {
			const extensionOptions = await this.#mediaTypeStructure.requestMediaTypesOf({ fileExtension });
			const mediaTypes = extensionOptions.filter((option) => {
				return allowedByParent.find((allowed) => option.unique === allowed.unique);
			});
			options.push({ fileExtension, mediaTypes });
		}
		return options;
	}

	async #showDialogMediaTypePicker(options: Array<UmbAllowedMediaTypeModel>) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this.#host, UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL, { data: { options } });
		const value = await modalContext.onSubmit().catch(() => undefined);
		return value ? { unique: value.mediaTypeUnique ?? options[0].unique } : null;
	}

	async #handleUpload(files: Array<UmbUploadableFileModel>) {
		const uploads = await this.#tempFileManager.upload(files);
		for (const upload of uploads) {
			if (upload.status !== TemporaryFileStatus.SUCCESS) return;
			// TODO: Create item
		}
	}

	private _reset() {
		//
	}

	public destroy() {
		super.destroy();
	}
}
