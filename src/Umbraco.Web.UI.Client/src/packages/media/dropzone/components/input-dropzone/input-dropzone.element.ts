import type { UmbUploadableItem } from '../../types.js';
import { UmbFileDropzoneItemStatus } from '../../constants.js';
import { UmbDropzoneManager } from '../../dropzone-manager.class.js';
import { UmbDropzoneChangeEvent } from '../../dropzone-change.event.js';
import { UmbDropzoneSubmittedEvent } from '../../dropzone-submitted.event.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	query,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { formatBytes } from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * A dropzone for uploading files and folders.
 * The files will be uploaded to the server as temporary files and can be used in the backoffice.
 * @element umb-input-dropzone
 * @fires ProgressEvent When the progress of the upload changes.
 * @fires UmbDropzoneChangeEvent When the upload is complete.
 * @fires UmbDropzoneSubmittedEvent When the upload is submitted.
 * @slot - The default slot.
 * @slot text - A text shown above the dropzone graphic.
 */
@customElement('umb-input-dropzone')
export class UmbInputDropzoneElement extends UmbFormControlMixin<UmbUploadableItem[], typeof UmbLitElement>(
	UmbLitElement,
) {
	/**
	 * Comma-separated list of accepted mime types or file extensions.
	 */
	@property({ type: String })
	accept?: string;

	/**
	 * Disable folder uploads.
	 */
	@property({ type: Boolean, attribute: 'disable-folder-upload', reflect: true })
	public set disableFolderUpload(isAllowed: boolean) {
		this.#manager.setIsFoldersAllowed(!isAllowed);
	}
	public get disableFolderUpload() {
		return this._disableFolderUpload;
	}
	private readonly _disableFolderUpload = false;

	/**
	 * Disables the dropzone.
	 * @description The dropzone will not accept any uploads.
	 */
	@property({ type: Boolean, reflect: true })
	disabled: boolean = false;

	/**
	 * Determines if the dropzone should accept multiple files.
	 */
	@property({ type: Boolean })
	multiple: boolean = false;

	/**
	 * The label for the dropzone.
	 */
	@property({ type: String })
	label = 'dropzone';

	@query('#dropzone', true)
	protected _dropzone?: UUIFileDropzoneElement;

	@state()
	protected _progressItems: Array<UmbUploadableItem> = [];

	#manager = new UmbDropzoneManager(this);

	constructor() {
		super();

		this.observe(
			this.#manager.progress,
			(progress) =>
				this.dispatchEvent(new ProgressEvent('progress', { loaded: progress.completed, total: progress.total })),
			'_observeProgress',
		);

		this.observe(
			this.#manager.progressItems,
			(progressItems) => {
				this._progressItems = [...progressItems];
				const waiting = this._progressItems.find((item) => item.status === UmbFileDropzoneItemStatus.WAITING);
				if (this._progressItems.length && !waiting) {
					this.value = [...this._progressItems];
					this.dispatchEvent(new UmbDropzoneChangeEvent(this._progressItems));
				}
			},
			'_observeProgressItems',
		);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#manager.destroy();
	}

	/**
	 * Opens the file browse dialog.
	 */
	public browse(): void {
		if (this.disabled) return;
		this._dropzone?.browse();
	}

	override render() {
		return html`
			<slot name="text"></slot>
			<uui-file-dropzone
				id="dropzone"
				label=${this.label}
				accept=${ifDefined(this.accept)}
				?multiple=${this.multiple}
				?disabled=${this.disabled}
				?disallowFolderUpload=${this.disableFolderUpload}
				@change=${this.onUpload}
				@click=${this.#handleBrowse}>
				<slot>
					<uui-button label=${this.localize.term('media_clickToUpload')} @click=${this.#handleBrowse}></uui-button>
				</slot>
			</uui-file-dropzone>
			${this.renderUploader()}
		`;
	}

	protected renderUploader() {
		if (this.disabled) return nothing;
		if (!this._progressItems?.length) return nothing;

		return html`
			<div id="uploader">
				${repeat(
					this._progressItems,
					(item) => item.unique,
					(item) => this.renderPlaceholder(item),
				)}
				<uui-button
					id="uploader-clear"
					compact
					@click=${this.#handleRemove}
					label=${this.localize.term('content_uploadClear')}>
					<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
				</uui-button>
			</div>
		`;
	}

	protected renderPlaceholder(item: UmbUploadableItem) {
		const file = item.temporaryFile?.file;
		return html`
			<div class="placeholder">
				<div class="fileIcon">
					${when(
						item.status === UmbFileDropzoneItemStatus.COMPLETE,
						() => html`<umb-icon name="check" color="green"></umb-icon>`,
					)}
					${when(
						item.status === UmbFileDropzoneItemStatus.ERROR ||
							item.status === UmbFileDropzoneItemStatus.CANCELLED ||
							item.status === UmbFileDropzoneItemStatus.NOT_ALLOWED,
						() => html`<umb-icon name="wrong" color="red"></umb-icon>`,
					)}
				</div>
				<div class="fileDetails">
					<div class="fileName" title=${file?.name ?? ''}>${file?.name ?? ''}</div>
					<div class="fileSize">
						${formatBytes(file?.size ?? 0, { decimals: 2 })}:
						${this.localize.number(item.progress, { maximumFractionDigits: 0 })}%
					</div>
					${when(
						item.status === UmbFileDropzoneItemStatus.WAITING,
						() => html`<div class="progress"><uui-loader-bar progress=${item.progress}></uui-loader-bar></div>`,
					)}
					${when(
						item.status === UmbFileDropzoneItemStatus.ERROR,
						() => html`<div class="error">An error occured</div>`,
					)}
					${when(item.status === UmbFileDropzoneItemStatus.CANCELLED, () => html`<div class="error">Cancelled</div>`)}
					${when(
						item.status === UmbFileDropzoneItemStatus.NOT_ALLOWED,
						() => html`<div class="error">File type not allowed</div>`,
					)}
				</div>
				<div class="fileActions">
					${when(
						item.status === UmbFileDropzoneItemStatus.WAITING,
						() => html`
							<uui-button
								compact
								@click=${() => this.#handleCancel(item)}
								label=${this.localize.term('general_cancel')}>
								<uui-icon name="icon-remove"></uui-icon>${this.localize.term('general_cancel')}
							</uui-button>
						`,
					)}
				</div>
			</div>
		`;
	}

	protected async onUpload(e: UUIFileDropzoneEvent) {
		e.stopImmediatePropagation();

		if (this.disabled) return;
		if (!e.detail.files.length && !e.detail.folders.length) return;

		const uploadables = this.#manager.createTemporaryFiles(e.detail.files);
		this.dispatchEvent(new UmbDropzoneSubmittedEvent(await uploadables));
	}

	#handleBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	#handleCancel(item: UmbUploadableItem) {
		item.temporaryFile?.abortController?.abort();
	}

	#handleRemove() {
		this.#manager.removeAll();
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				flex-wrap: wrap;
				place-items: center;
				cursor: pointer;
			}

			:host([disabled]) #dropzone {
				opacity: 0.5;
				pointer-events: none;
			}

			#dropzone {
				width: 100%;
				inset: 0;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */
			}

			#uploader {
				display: flex;
				flex-direction: column;
				flex-wrap: wrap;
				align-items: center;
				gap: var(--uui-size-space-3);

				.placeholder {
					display: grid;
					grid-template-columns: 30px 200px 1fr;
					max-width: fit-content;
					padding: var(--uui-size-space-3);
					border: 1px dashed var(--uui-color-divider-emphasis);
				}

				.fileIcon,
				.fileActions {
					place-self: center center;
				}

				.fileName {
					white-space: nowrap;
					overflow: hidden;
					text-overflow: ellipsis;
					font-size: var(--uui-size-5);
				}

				.fileSize {
					font-size: var(--uui-font-size-small);
					color: var(--uui-color-text-alt);
				}

				.error {
					color: var(--uui-color-danger);
				}
			}
		`,
	];
}

export const UmbInputDropzoneDashedStyles = css`
	umb-input-dropzone {
		position: relative;
		display: block;
		inset: 0;
		cursor: pointer;
		border: 1px dashed var(--uui-color-divider-emphasis);
	}
`;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dropzone': UmbInputDropzoneElement;
	}
}
