import { UmbMediaTypeImportRepository } from '../repository/media-type-import.repository.js';
import type { UmbMediaTypeImportModalData, UmbMediaTypeImportModalValue } from './media-type-import-modal.token.js';
import { css, html, customElement, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbDropzoneElement } from '@umbraco-cms/backoffice/media';

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

	@query('#dropzone')
	private dropzone?: UmbDropzoneElement;

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

	#onUploadCompleted() {
		const data = this.dropzone?.getItems()[0];
		if (!data?.temporaryFile) return;

		this.#temporaryUnique = data.temporaryFile.temporaryUnique;
		this.#fileReader.readAsText(data.temporaryFile.file);
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

	async #onBrowse() {
		this.dropzone?.browse();
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
					/**TODO Add localizations */
					html`<div id="wrapper">
						Drag and drop your file here
						<uui-button look="primary" label="or click here to choose a file" @click=${this.#onBrowse}></uui-button>
						<umb-dropzone
							id="dropzone"
							accept=".udt"
							@complete=${this.#onUploadCompleted}
							createAsTemporary></umb-dropzone>
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
