import type { UmbMediaDetailModel } from '../types.js';
import { UmbMediaDetailRepository } from '../repository/index.js';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from './modals/dropzone-media-type-picker/dropzone-media-type-picker-modal.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbAllowedMediaTypeModel, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbUploadableFileModel extends UmbTemporaryFileModel {
	unique: string;
	mediaTypeUnique: string;
}

export interface UmbUploadableExtensionModel {
	fileExtension: string;
	mediaTypes: Array<UmbAllowedMediaTypeModel>;
}

/**
 * Manages the dropzone and uploads files to the server.
 * @method createFilesAsMedia - Upload files to the server and creates the items using corresponding media type.
 * @method createFilesAsTemporary - Upload the files as temporary files and returns the data.
 * @observable completed - Emits an array of completed uploads.
 */
export class UmbDropzoneManager extends UmbControllerBase {
	#host;

	#tempFileManager = new UmbTemporaryFileManager(this);

	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	#completed = new UmbArrayState<UmbUploadableFileModel | UmbTemporaryFileModel>(
		[],
		(upload) => upload.temporaryUnique,
	);
	public readonly completed = this.#completed.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.#host = host;
	}

	/**
	 * Uploads the files as temporary files and returns the data.
	 * @param files
	 * @returns Promise<Array<UmbUploadableFileModel>>
	 */
	public async createFilesAsTemporary(files: Array<File>): Promise<Array<UmbTemporaryFileModel>> {
		this.#completed.setValue([]);
		const temporaryFiles: Array<UmbTemporaryFileModel> = [];

		for (const file of files) {
			const uploaded = await this.#tempFileManager.uploadOne({ temporaryUnique: UmbId.new(), file });
			this.#completed.setValue([...this.#completed.getValue(), uploaded]);
			temporaryFiles.push(uploaded);
		}

		return temporaryFiles;
	}

	/**
	 * Uploads files to the server and creates the items with corresponding media type.
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param files
	 * @param parentUnique
	 */
	public async createFilesAsMedia(files: Array<File>, parentUnique: string | null) {
		if (!files.length) return;
		if (files.length === 1) return this.#handleOneOneFile(files[0], parentUnique);

		// Handler for multiple files dropped

		this.#completed.setValue([]);
		// removes duplicate file types so we don't call endpoints unnecessarily when building options.
		const mimeTypes = [...new Set(files.map<string>((file) => file.type))];
		const optionsArray = await this.#buildOptionsArrayFrom(
			mimeTypes.map((mimetype) => this.#getExtensionFromMime(mimetype)),
			parentUnique,
		);

		if (!optionsArray.length) return; // None of the files are allowed in current dropzone.

		// Building an array of uploadable files. Do we want to build an array of failed files to let the user know which ones?
		const uploadableFiles: Array<UmbUploadableFileModel> = [];
		const notAllowedFiles: Array<File> = [];

		for (const file of files) {
			const extension = this.#getExtensionFromMime(file.type);
			if (!extension) {
				// Folders have no extension on file drop. We assume it is a folder being uploaded.
				continue;
			}
			const options = optionsArray.find((option) => option.fileExtension === extension)?.mediaTypes;

			if (!options || !options.length) {
				// TODO Current dropped file not allowed in this area. Find a good way to show this to the user after we finish uploading the rest of the files.
				notAllowedFiles.push(file);
				continue;
			}

			// Since we are uploading multiple files, we will pick first allowed option.
			// Consider a way we can handle this differently in the future to let the user choose. Maybe a list of all files with an allowed media type dropdown?
			const mediaType = options[0];
			uploadableFiles.push({
				temporaryUnique: UmbId.new(),
				file,
				mediaTypeUnique: mediaType.unique,
				unique: UmbId.new(),
			});
		}

		notAllowedFiles.forEach((file) => {
			try {
				throw new Error(`File ${file.name} of type ${file.type} is not allowed here.`);
			} catch (e) {
				undefined;
			}
		});

		if (!uploadableFiles.length) return;

		await this.#handleUpload(uploadableFiles, parentUnique);
	}

	async #handleOneOneFile(file: File, parentUnique: string | null) {
		this.#completed.setValue([]);
		const extension = this.#getExtensionFromMime(file.type);

		if (!extension) {
			// TODO Folders have no extension on file drop. Assume it is a folder being uploaded.
			return;
		}

		const optionsArray = await this.#buildOptionsArrayFrom([extension], parentUnique);
		if (!optionsArray.length || !optionsArray[0].mediaTypes.length) {
			throw new Error(`File ${file.name} of type ${file.type} is not allowed here.`); // Parent does not allow this file type here.
		}

		const mediaTypes = optionsArray[0].mediaTypes;
		if (mediaTypes.length === 1) {
			// Only one allowed option, upload file using that option.
			const uploadableFile: UmbUploadableFileModel = {
				unique: UmbId.new(),
				temporaryUnique: UmbId.new(),
				file,
				mediaTypeUnique: mediaTypes[0].unique,
			};

			await this.#handleUpload([uploadableFile], parentUnique);
			return;
		}

		// Multiple options, show a dialog for the user to pick one.
		const mediaType = await this.#showDialogMediaTypePicker(mediaTypes);
		if (!mediaType) return; // Upload cancelled.

		const uploadableFile: UmbUploadableFileModel = {
			unique: UmbId.new(),
			temporaryUnique: UmbId.new(),
			file,
			mediaTypeUnique: mediaType.unique,
		};
		await this.#handleUpload([uploadableFile], parentUnique);
	}

	#getExtensionFromMime(mime: string): string {
		//TODO Temporary solution.
		if (!mime) return ''; //folders
		const extension = mime.split('/')[1];
		switch (extension) {
			case 'svg+xml':
				return 'svg';
			default:
				return extension;
		}
	}

	async #buildOptionsArrayFrom(
		fileExtensions: Array<string>,
		parentUnique: string | null,
	): Promise<Array<UmbUploadableExtensionModel>> {
		let parentMediaType: string | null = null;
		if (parentUnique) {
			const { data } = await this.#mediaDetailRepository.requestByUnique(parentUnique);
			parentMediaType = data?.mediaType.unique ?? null;
		}

		// Getting all media types allowed in our current position based on parent's media type.

		const { data: allAllowedMediaTypes } = await this.#mediaTypeStructure.requestAllowedChildrenOf(parentMediaType);
		if (!allAllowedMediaTypes?.items.length) return [];

		const allowedByParent = allAllowedMediaTypes.items;

		// Building an array of options the files can be uploaded as.
		const options: Array<UmbUploadableExtensionModel> = [];

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

	async #handleUpload(files: Array<UmbUploadableFileModel>, parentUnique: string | null) {
		for (const file of files) {
			const upload = (await this.#tempFileManager.uploadOne(file)) as UmbUploadableFileModel;

			if (upload.status === TemporaryFileStatus.SUCCESS) {
				// Upload successful. Create media item.
				const preset: Partial<UmbMediaDetailModel> = {
					unique: file.unique,
					mediaType: {
						unique: upload.mediaTypeUnique,
						collection: null,
					},
					variants: [
						{
							culture: null,
							segment: null,
							name: upload.file.name,
							createDate: null,
							updateDate: null,
						},
					],
					values: [
						{
							alias: 'umbracoFile',
							value: { temporaryFileId: upload.temporaryUnique },
							culture: null,
							segment: null,
						},
					],
				};
				const { data } = await this.#mediaDetailRepository.createScaffold(preset);
				await this.#mediaDetailRepository.create(data!, parentUnique);
			}
			// TODO Find a good way to show files that ended up as TemporaryFileStatus.ERROR. Notice that they were allowed in current area

			this.#completed.setValue([...this.#completed.getValue(), upload]);
		}
	}

	private _reset() {
		//
	}

	public destroy() {
		this.#tempFileManager.destroy();
		this.#completed.destroy();
		super.destroy();
	}
}
