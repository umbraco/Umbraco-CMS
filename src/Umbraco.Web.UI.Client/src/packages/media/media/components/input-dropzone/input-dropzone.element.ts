import { UmbDropzoneChangeEvent, UmbDropzoneManager, UmbDropzoneSubmittedEvent } from '../../dropzone/index.js';
import { UmbFileDropzoneItemStatus, type UmbUploadableItem } from '../../dropzone/types.js';
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
 * @element umb-input-dropzone
 * @fires ProgressEvent When the progress of the upload changes.
 * @fires UmbDropzoneChangeEvent When the upload is complete.
 * @fires UmbDropzoneSubmittedEvent When the upload is submitted.
 * @slot - The default slot.
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
	 * Determines if the dropzone should create temporary files or media items directly.
	 */
	@property({ type: Boolean })
	createAsTemporary: boolean = false;

	/**
	 * Create the media item below this parent.
	 * @description This is only used when `createAsTemporary` is `false`.
	 */
	@property({ attribute: false })
	parentUnique: string | null = null;

	/**
	 * Disables the dropzone.
	 * @description The dropzone will not accept any uploads.
	 */
	@property({ type: Boolean, reflect: true })
	disabled: boolean = false;

	/**
	 * The label for the dropzone.
	 */
	@property({ type: String })
	label = 'dropzone';

	@query('#dropzone', true)
	private _dropzone?: UUIFileDropzoneElement;

	@state()
	private _progressItems?: Array<UmbUploadableItem>;

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

	override render() {
		return html`
			<uui-file-dropzone
				id="dropzone"
				label=${this.label}
				disallowFolderUpload
				accept=${ifDefined(this.accept)}
				@change=${this.#onUpload}
				@click=${this.#handleBrowse}>
				<slot>
					<uui-button label=${this.localize.term('media_clickToUpload')} @click=${this.#handleBrowse}></uui-button>
				</slot>
			</uui-file-dropzone>
			${this.#renderUploader()}
		`;
	}

	#renderUploader() {
		if (!this._progressItems?.length) return nothing;

		return html`
			${repeat(
				this._progressItems,
				(item) => item.unique,
				(item) => this.#renderPlaceholder(item),
			)}
		`;
	}

	#renderPlaceholder(item: UmbUploadableItem) {
		const file = item.temporaryFile?.file;
		return html`
			<div id="temporaryFile">
				<div id="fileIcon">
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
				<div id="fileDetails">
					<div id="fileName">${file?.name}</div>
					<div id="fileSize">${formatBytes(file?.size ?? 0, { decimals: 2 })}: ${item.progress}%</div>
					${when(
						item.status === UmbFileDropzoneItemStatus.WAITING,
						() => html`<div id="progress"><uui-loader-bar progress=${item.progress}></uui-loader-bar></div>`,
					)}
					${when(item.status === UmbFileDropzoneItemStatus.ERROR, () => html`<div id="error">An error occured</div>`)}
					${when(item.status === UmbFileDropzoneItemStatus.CANCELLED, () => html`<div id="error">Cancelled</div>`)}
					${when(
						item.status === UmbFileDropzoneItemStatus.NOT_ALLOWED,
						() => html`<div id="error">File type not allowed</div>`,
					)}
				</div>
				<div id="fileActions">
					${when(
						item.status === UmbFileDropzoneItemStatus.WAITING,
						() => html`
							<uui-button
								compact
								@click=${() => this.#handleRemove(item)}
								label=${this.localize.term('general_cancel')}>
								<uui-icon name="remove"></uui-icon>${this.localize.term('general_cancel')}
							</uui-button>
						`,
						() => this.#renderButtonRemove(),
					)}
				</div>
			</div>
		`;
	}

	#renderButtonRemove() {
		return html`
			<uui-button compact @click=${this.#handleRemove} label=${this.localize.term('content_uploadClear')}>
				<uui-icon name="icon-trash"></uui-icon>${this.localize.term('content_uploadClear')}
			</uui-button>
		`;
	}

	#handleBrowse(e: Event) {
		if (!this._dropzone) return;
		e.stopImmediatePropagation();
		this._dropzone.browse();
	}

	#handleRemove(item?: UmbUploadableItem) {
		// Remove single item
		if (item) {
			item.temporaryFile?.abortController?.abort();
			const index = this._progressItems?.indexOf(item);
			if (index !== undefined && index !== -1) {
				this._progressItems?.splice(index, 1);
				this.requestUpdate('_progressItems');
			}
			return;
		}

		// Remove all items
		if (this._progressItems?.length) {
			for (const item of this._progressItems) {
				item.temporaryFile?.abortController?.abort();
			}

			this._progressItems.length = 0;
			this.requestUpdate('_progressItems');
		}
	}

	async #onUpload(e: UUIFileDropzoneEvent) {
		e.stopPropagation();

		if (this.disabled) return;
		if (!e.detail.files.length && !e.detail.folders.length) return;

		if (this.createAsTemporary) {
			const uploadables = this.#manager.createTemporaryFiles(e.detail.files);
			this.dispatchEvent(new UmbDropzoneSubmittedEvent(await uploadables));
		} else {
			const uploadables = this.#manager.createMediaItems(e.detail, null);
			this.dispatchEvent(new UmbDropzoneSubmittedEvent(uploadables));
		}
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host([disabled]) #dropzone {
				opacity: 0.5;
				pointer-events: none;
			}
		`,
	];
}

export { UmbInputDropzoneElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dropzone': UmbInputDropzoneElement;
	}
}
