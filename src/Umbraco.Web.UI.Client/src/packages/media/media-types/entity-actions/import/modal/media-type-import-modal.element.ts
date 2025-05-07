import { UmbMediaTypeImportRepository } from '../repository/media-type-import.repository.js';
import type { UmbMediaTypeImportModalData, UmbMediaTypeImportModalValue } from './media-type-import-modal.token.js';
import { css, html, customElement, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbDropzoneMediaElement } from '@umbraco-cms/backoffice/media';
import type { UmbDropzoneChangeEvent } from '@umbraco-cms/backoffice/dropzone';

interface UmbMediaTypePreview {
	unique: string;
	name: string;
	alias: string;
	icon: string;
}

@customElement('umb-media-type-import-modal')
export class UmbMediaTypeImportModalLayout extends UmbModalBaseElement<
	UmbMediaTypeImportModalData,
	UmbMediaTypeImportModalValue
> {
	#MediaTypeImportRepository = new UmbMediaTypeImportRepository(this);
	#temporaryUnique?: string;
	#fileReader;

	@state()
	private _fileContent: Array<UmbMediaTypePreview> = [];

	constructor() {
		super();
		this.#fileReader = new FileReader();
		this.#fileReader.onload = (e) => {
			if (typeof e.target?.result === 'string') {
				const fileContent = e.target.result;
				this.#mediaTypePreviewBuilder(fileContent);
			} else {
				this.#requestReset();
			}
		};
	}

	#onUploadComplete(evt: UmbDropzoneChangeEvent) {
		evt.preventDefault();
		const target = evt.target as UmbDropzoneMediaElement;
		const data = target.value;
		if (!data?.length) return;

		const file = data[0];

		if (file.temporaryFile) {
			this.#temporaryUnique = file.temporaryFile.temporaryUnique;
			this.#fileReader.readAsText(file.temporaryFile.file);
		}
	}

	async #onFileImport() {
		if (!this.#temporaryUnique) return;
		const { error } = await this.#MediaTypeImportRepository.requestImport(this.#temporaryUnique);
		if (error) return;
		this._submitModal();
	}

	#mediaTypePreviewBuilder(htmlString: string) {
		const parser = new DOMParser();
		const doc = parser.parseFromString(htmlString, 'text/xml');
		const childNodes = doc.childNodes;

		const elements: Array<Element> = [];

		childNodes.forEach((node) => {
			if (node.nodeType === Node.ELEMENT_NODE && node.nodeName === 'MediaType') {
				elements.push(node as Element);
			}
		});

		this._fileContent = this.#mediaTypePreviewItemBuilder(elements);
	}

	#mediaTypePreviewItemBuilder(elements: Array<Element>) {
		const mediaTypes: Array<UmbMediaTypePreview> = [];
		elements.forEach((MediaType) => {
			const info = MediaType.getElementsByTagName('Info')[0];
			const unique = info.getElementsByTagName('Key')[0].textContent ?? '';
			const name = info.getElementsByTagName('Name')[0].textContent ?? '';
			const alias = info.getElementsByTagName('Alias')[0].textContent ?? '';
			const icon = info.getElementsByTagName('Icon')[0].textContent ?? '';

			mediaTypes.push({ unique, name, alias, icon });
		});
		return mediaTypes;
	}

	#requestReset() {
		this._fileContent = [];
		this.#temporaryUnique = undefined;
	}

	override render() {
		return html` <umb-body-layout headline=${this.localize.term('general_import')}>
			<uui-box> ${this.#renderUploadZone()} </uui-box>
			<uui-button
				slot="actions"
				type="button"
				label=${this.localize.term('general_cancel')}
				@click=${this._rejectModal}></uui-button>
			<uui-button
				slot="actions"
				type="button"
				look="primary"
				?disabled=${!this.#temporaryUnique}
				label=${this.localize.term('actions_import')}
				@click=${this.#onFileImport}></uui-button>
		</umb-body-layout>`;
	}

	#renderUploadZone() {
		return html`
			${when(
				this._fileContent.length,
				() =>
					html`<uui-ref-node name=${this._fileContent[0].name} alias=${this._fileContent[0].alias} readonly standalone>
						<umb-icon slot="icon" name=${this._fileContent[0].icon}></umb-icon>
						<uui-button
							slot="actions"
							@click=${this.#requestReset}
							label=${this.localize.term('general_remove')}></uui-button>
					</uui-ref-node>`,
				() =>
					html`<div id="wrapper">
						<umb-input-dropzone id="dropzone" accept=".udt" @change=${this.#onUploadComplete}
							><umb-localize slot="text" key="media_dragAndDropYourFilesIntoTheArea"
								>Drag and drop your file(s) into the area
							</umb-localize></umb-input-dropzone
						>
					</div>`,
			)}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#wrapper {
				display: flex;
				flex-direction: column;
				align-items: center;
				text-align: center;
				position: relative;
				gap: var(--uui-size-space-3);
				border: 2px dashed var(--uui-color-divider-standalone);
				background-color: var(--uui-color-surface-alt);
				padding: var(--uui-size-space-6);
			}

			#dropzone {
				width: 100%;
			}

			#import {
				margin-top: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbMediaTypeImportModalLayout;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-import-modal': UmbMediaTypeImportModalLayout;
	}
}
