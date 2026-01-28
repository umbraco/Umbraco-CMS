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
	@property({ type: Boolean, attribute: 'disable-folder-upload' })
	disableFolderUpload: boolean = false;

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
	 * Style the dropzone with a border.
	 * @description This is useful if you want to display the dropzone directly rather than as a part of a separate component.
	 */
	@property({ type: Boolean, reflect: true })
	standalone: boolean = false;

	/**
	 * The label for the dropzone.
	 */
	@property({ type: String })
	label = 'dropzone';

	@query('#dropzone', true)
	protected _dropzone?: UUIFileDropzoneElement;

	@query('#uploader')
	protected _uploader?: HTMLDivElement;

	@state()
	protected _progressItems: Array<UmbUploadableItem> = [];

	protected _manager = new UmbDropzoneManager(this);

	#autoCloseTimeout?: ReturnType<typeof setTimeout>;

	/**
	 * Determines if the dropzone should be disabled.
	 * If the dropzone is disabled, it will not accept any uploads.
	 * It is considered disabled if the `disabled` property is set or if `multiple` is set to `false` and there is already an upload in progress.
	 * @returns {boolean} True if the dropzone should not accept uploads, otherwise false.
	 */
	get #isDisabled(): boolean {
		return this.disabled || (!this.multiple && this._progressItems.length > 0);
	}

	constructor() {
		super();
		this._observeProgress();
		this._observeProgressItems();
	}

	protected _observeProgress() {
		this.observe(
			this._manager.progress,
			(progress) => {
				this.dispatchEvent(new ProgressEvent('progress', { loaded: progress.completed, total: progress.total }));
			},
			'_observeProgress',
		);
	}

	protected _observeProgressItems() {
		this.observe(
			this._manager.progressItems,
			async (progressItems) => {
				this._progressItems = [...progressItems];
				const hasWaiting = this._progressItems.some((item) => item.status === UmbFileDropzoneItemStatus.WAITING);

				// Clear any pending auto-close when new uploads start
				if (hasWaiting) {
					clearTimeout(this.#autoCloseTimeout);
				}

				this.#handleUploadComplete(hasWaiting);
				await this.#showPopover();
			},
			'_observeProgressItems',
		);
	}

	#handleUploadComplete(hasWaiting: boolean): void {
		if (!this._progressItems.length || hasWaiting) return;

		this.value = [...this._progressItems];
		this.dispatchEvent(new UmbDropzoneChangeEvent(this._progressItems));

		// Only auto-close if all items completed successfully (no errors)
		const allSuccessful = this._progressItems.every((item) => item.status === UmbFileDropzoneItemStatus.COMPLETE);
		if (!allSuccessful) return;

		this.#autoCloseTimeout = setTimeout(() => {
			this._manager.removeAll();
		}, 750);
	}

	async #showPopover(): Promise<void> {
		if (!this._progressItems.length) return;

		await this.updateComplete;
		if (this._uploader && !this._uploader.matches(':popover-open')) {
			this._uploader.showPopover();
		}
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		clearTimeout(this.#autoCloseTimeout);
		this._manager.destroy();
	}

	/**
	 * Opens the file browse dialog.
	 */
	public browse(): void {
		if (this.#isDisabled) return;
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
				?disabled=${this.#isDisabled}
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
		if (!this._progressItems?.length) return nothing;

		return html`
			<div id="uploader" popover="manual">
				<div id="uploader-header">
					<span><umb-localize key="media_uploading">Uploading</umb-localize></span>
					<uui-button
						id="uploader-close"
						compact
						@click=${this.#handleRemove}
						label=${this.localize.term('general_close')}>
						<uui-icon name="remove"></uui-icon>
					</uui-button>
				</div>
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
					<uui-icon name="icon-trash"></uui-icon>
					<umb-localize key="content_uploadClear">Clear file(s)</umb-localize>
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

		if (this.#isDisabled) return;
		if (!e.detail.files.length && !e.detail.folders.length) return;

		const uploadables = this._manager.createTemporaryFiles(e.detail.files);
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
		this._manager.removeAll();
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

			:host([hidden]) {
				display: none;
			}

			:host([disabled]) #dropzone {
				opacity: 0.5;
				pointer-events: none;
			}

			:host([standalone]) {
				position: relative;
				display: block;
				inset: 0;
				cursor: pointer;
				border: 1px dashed var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
			}

			:host([standalone]:not([disabled]):hover) {
				border-color: var(--uui-color-default-emphasis);
				--uui-color-default: var(--uui-color-default-emphasis);
				color: var(--uui-color-default-emphasis);
			}

			#dropzone {
				width: 100%;
				inset: 0;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */

				&[disabled] {
					opacity: 0.5;
					pointer-events: none;
				}
			}

			#uploader {
				display: flex;
				flex-direction: column;
				flex-wrap: wrap;
				align-items: center;
				gap: var(--uui-size-space-3);
				background: var(--umb-dropzone-uploader-background, var(--uui-color-surface));
				padding: var(--umb-dropzone-uploader-padding, var(--uui-size-space-5));
				border-radius: var(--umb-dropzone-uploader-border-radius, var(--uui-border-radius));
				border: var(--umb-dropzone-uploader-border, 1px solid var(--uui-color-border));
				box-shadow: var(--umb-dropzone-uploader-box-shadow, var(--uui-shadow-depth-3));
				max-width: var(--umb-dropzone-uploader-max-width, 350px);
				cursor: default;

				/* Popover positioning - uses inset: auto to reset default centering */
				&:popover-open {
					inset: auto;
					top: var(--umb-dropzone-uploader-top, auto);
					bottom: var(--umb-dropzone-uploader-bottom, 24px);
					left: var(--umb-dropzone-uploader-left, auto);
					right: var(--umb-dropzone-uploader-right, 24px);
				}

				&::backdrop {
					background: transparent;
				}

				#uploader-header {
					display: flex;
					justify-content: space-between;
					align-items: center;
					width: 100%;
					font-weight: bold;
					padding-bottom: var(--uui-size-space-2);
					border-bottom: 1px solid var(--uui-color-divider);
					margin-bottom: var(--uui-size-space-2);
				}

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

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dropzone': UmbInputDropzoneElement;
	}
}
