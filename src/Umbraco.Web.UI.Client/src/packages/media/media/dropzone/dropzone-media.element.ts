import { UmbDropzoneMediaManager } from './dropzone-media-manager.class.js';
import {
	UmbInputDropzoneElement,
	UmbFileDropzoneItemStatus,
	UmbDropzoneSubmittedEvent,
	type UmbUploadableItem,
	type UmbFileDropzoneDroppedItems,
} from '@umbraco-cms/backoffice/dropzone';
import { css, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * A dropzone for uploading files and folders as media items. It is hidden by default and will be shown when dragging files over the window.
 * @element umb-dropzone-media
 * @fires ProgressEvent When the progress of the upload changes.
 * @fires UmbDropzoneSubmittedEvent When the upload is submitted.
 * @fires UmbDropzoneChangeEvent When any upload changes.
 * @fires CustomEvent<'complete'> When all uploads are complete (deprecated: use {@link UmbDropzoneChangeEvent} instead).
 * @slot - The default slot.
 */
@customElement('umb-dropzone-media')
export class UmbDropzoneMediaElement extends UmbInputDropzoneElement {
	/**
	 * Gets the current value of the uploaded items.
	 * @returns {Array<UmbUploadableItem>} An array of uploadable items.
	 */
	public getItems(): Array<UmbUploadableItem> {
		return this._progressItems;
	}

	public progressItems = () => this._manager.progressItems;
	public progress = () => this._manager.progress;

	#mediaManager = new UmbDropzoneMediaManager(this);

	constructor() {
		super();

		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));

		this.observe(
			this._manager.progressItems,
			(progressItems) => {
				const waiting = progressItems.find((item) => item.status === UmbFileDropzoneItemStatus.WAITING);
				if (progressItems.length && !waiting) {
					this.dispatchEvent(new CustomEvent('complete', { detail: progressItems }));
				}
			},
			'_observeProgressItemsComplete',
		);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	override async onUpload(event: UUIFileDropzoneEvent) {
		event.stopImmediatePropagation();

		if (this.disabled) return;
		if (!event.detail.files.length && !event.detail.folders.length) return;

		const droppedItems: UmbFileDropzoneDroppedItems = {
			files: event.detail.files,
			folders: event.detail.folders,
		};

		const uploadableItems = await this._manager.createTemporaryFiles(droppedItems, this.parentUnique);
		const uploadables = this.#mediaManager.createMediaItems(uploadableItems);
		this.dispatchEvent(new UmbDropzoneSubmittedEvent(uploadables));
	}

	#handleDragEnter(e: DragEvent) {
		if (this.disabled) return;
		// Avoid collision with UmbSorterController
		const types = e.dataTransfer?.types;
		if (!types?.length || !types?.includes('Files')) return;

		this.toggleAttribute('dragging', true);
	}

	#handleDragLeave() {
		if (this.disabled) return;
		this.toggleAttribute('dragging', false);
	}

	#handleDrop(event: DragEvent) {
		event.preventDefault();
		if (this.disabled) return;
		this.toggleAttribute('dragging', false);
	}

	static override styles = [
		...UmbInputDropzoneElement.styles,
		css`
			:host(:not([disabled])[dragging]) #dropzone {
				opacity: 1;
				pointer-events: all;
			}

			[dropzone] {
				opacity: 0;
			}

			#dropzone {
				opacity: 0;
				pointer-events: none;
				display: flex;
				align-items: center;
				justify-content: center;
				position: absolute;
				z-index: 100;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-focus);
			}
		`,
	];
}

export default UmbDropzoneMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone-media': UmbDropzoneMediaElement;
	}
}
