import { UmbTiptapExtensionApi } from './types.js';
import {
	TemporaryFileStatus,
	UmbTemporaryFileManager,
	type UmbTemporaryFileModel,
} from '@umbraco-cms/backoffice/temporary-file';
import { type Editor, UmbImage } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export default class UmbTiptapMediaUploadExtension extends UmbTiptapExtensionApi {
	/**
	 * TODO: Implement this method when the configuration is available to extensions
	 * @returns The maximum width of uploaded images
	 */
	get maxWidth() {
		return 500;
	}

	/**
	 * TODO: Implement this method when the configuration is available to extensions
	 * @returns The allowed file types for uploads
	 */
	get allowedFileTypes() {
		return ['image/jpeg', 'image/png', 'image/gif'];
	}

	#manager = new UmbTemporaryFileManager(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	getTiptapExtensions() {
		// eslint-disable-next-line @typescript-eslint/no-this-alias
		const self = this;
		return [
			UmbImage.extend({
				name: 'umbMediaUpload',
				addAttributes() {
					return {
						...this.parent?.(),
						'data-tmpimg': { default: null },
					};
				},
				onCreate() {
					this.parent?.();
					const host = this.editor.view.dom;

					host.addEventListener('dragover', (event) => {
						// Required to allow drop events
						event.preventDefault();
					});

					host.addEventListener('drop', (event) => {
						event.preventDefault();

						const files = event.dataTransfer?.files;
						if (!files) return;

						self.#uploadTemporaryFile(files, this.editor);
					});
				},
			}),
		];
	}

	async #uploadTemporaryFile(files: FileList, editor: Editor) {
		const filteredFiles = this.#filterFiles(files);
		const fileModels = filteredFiles.map((file) => this.#mapFileToTemporaryFile(file));

		this.dispatchEvent(new CustomEvent('rte.file.uploading', { composed: true, bubbles: true, detail: fileModels }));

		const uploads = await this.#manager.upload(fileModels);

		uploads.forEach((upload) => {
			if (upload.status !== TemporaryFileStatus.SUCCESS) {
				this.#notificationContext?.peek('danger', {
					data: {
						headline: upload.file.name,
						message: this.#localize.term('errors_dissallowedMediaType'),
					},
				});
				return;
			}

			editor
				.chain()
				.focus()
				.setImage({
					src: URL.createObjectURL(upload.file),
					width: this.maxWidth.toString(),
					'data-tmpimg': upload.temporaryUnique,
				})
				.run();
		});

		this.dispatchEvent(new CustomEvent('rte.file.uploaded', { composed: true, bubbles: true, detail: uploads }));
	}

	#mapFileToTemporaryFile(file: File): UmbTemporaryFileModel {
		return {
			file,
			temporaryUnique: UmbId.new(),
		};
	}

	#filterFiles(files: FileList): File[] {
		return Array.from(files).filter((file) => this.allowedFileTypes.includes(file.type));
	}
}

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbMediaUpload: {
			/**
			 * Add an image
			 * @param options The image attributes
			 * @example
			 * editor
			 *   .commands
			 *   .setImage({ src: 'https://tiptap.dev/logo.png', alt: 'tiptap', title: 'tiptap logo' })
			 */
			setImage: (options: {
				src: string;
				alt?: string;
				title?: string;
				width?: string;
				height?: string;
				loading?: string;
				srcset?: string;
				sizes?: string;
				'data-tmpimg'?: string;
			}) => ReturnType;
		};
	}
}
