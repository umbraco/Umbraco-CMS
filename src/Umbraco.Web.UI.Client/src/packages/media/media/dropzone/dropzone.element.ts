import { UmbDropzoneManager, type UmbUploadableFileModel } from './dropzone-manager.class.js';
import { UmbProgressEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIFileDropzoneElement, UUIFileDropzoneEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTemporaryFileModel } from '@umbraco-cms/backoffice/temporary-file';

@customElement('umb-dropzone')
export class UmbDropzoneElement extends UmbLitElement {
	@property({ attribute: false })
	parentUnique: string | null = null;

	@property({ type: Boolean })
	multiple: boolean = true;

	@property({ type: Boolean })
	createAsTemporary: boolean = false;

	@property({ type: Array, attribute: false })
	accept: Array<string> = [];

	//TODO: logic to disable the dropzone?

	#files: Array<UmbUploadableFileModel | UmbTemporaryFileModel> = [];

	public getFiles() {
		return this.#files;
	}

	public browse() {
		const element = this.shadowRoot?.querySelector('#dropzone') as UUIFileDropzoneElement;
		return element.browse();
	}

	constructor() {
		super();
		document.addEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.addEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.addEventListener('drop', this.#handleDrop.bind(this));
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this.#handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this.#handleDragLeave.bind(this));
		document.removeEventListener('drop', this.#handleDrop.bind(this));
	}

	#handleDragEnter(e: DragEvent) {
		// Avoid collision with UmbSorterController
		const types = e.dataTransfer?.types;
		if (!types?.length || !types?.includes('Files')) return;
		this.toggleAttribute('dragging', true);
	}

	#handleDragLeave() {
		this.toggleAttribute('dragging', false);
	}

	#handleDrop(event: DragEvent) {
		event.preventDefault();
		this.toggleAttribute('dragging', false);
	}

	async #onDropFiles(event: UUIFileDropzoneEvent) {
		// TODO Handle of folder uploads.

		const files: Array<File> = event.detail.files;
		if (!files.length) return;

		const dropzoneManager = new UmbDropzoneManager(this);
		this.observe(
			dropzoneManager.completed,
			(completed) => {
				if (!completed.length) return;

				const progress = Math.floor(completed.length / files.length);
				this.dispatchEvent(new UmbProgressEvent(progress));

				if (completed.length === files.length) {
					this.#files = completed;
					this.dispatchEvent(new CustomEvent('change', { detail: { completed } }));
					dropzoneManager.destroy();
				}
			},
			'_observeCompleted',
		);
		//TODO Create some placeholder items while files are being uploaded? Could update them as they get completed.
		if (this.createAsTemporary) {
			await dropzoneManager.createFilesAsTemporary(files);
		} else {
			await dropzoneManager.createFilesAsMedia(files, this.parentUnique);
		}
	}

	render() {
		return html`<uui-file-dropzone
			id="dropzone"
			.accept=${this.accept?.join(',')}
			?multiple=${this.multiple}
			@change=${this.#onDropFiles}
			label="${this.localize.term('media_dragAndDropYourFilesIntoTheArea')}"></uui-file-dropzone>`;
	}

	static styles = [
		css`
			:host([dragging]) #dropzone {
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
				inset: 0px;
				z-index: 100;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */
				border-radius: var(--uui-border-radius);
				overflow: clip;
				border: 1px solid var(--uui-color-focus);
			}
			#dropzone:after {
				content: '';
				display: block;
				position: absolute;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-focus);
				opacity: 0.2;
			}
		`,
	];
}

export default UmbDropzoneElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone': UmbDropzoneElement;
	}
}
