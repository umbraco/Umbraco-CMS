import '../../components/dictionary-item-input/dictionary-item-input.element.js';
import UmbDictionaryItemInputElement from '../../components/dictionary-item-input/dictionary-item-input.element.js';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { css, html, customElement, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

interface DictionaryItemPreview {
	name: string;
	children: Array<DictionaryItemPreview>;
}

@customElement('umb-import-dictionary-modal')
export class UmbImportDictionaryModalLayout extends UmbModalBaseElement<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue
> {
	@state()
	private _parentId?: string;

	@state()
	private _temporaryFileId?: string;

	@query('#form')
	private _form!: HTMLFormElement;

	#fileReader;

	#fileContent: Array<DictionaryItemPreview> = [];

	#handleClose() {
		this.modalContext?.reject();
	}

	#submit() {
		// TODO: Gotta do a temp file upload before submitting, so that the server can use it
		console.log('submit:', this._temporaryFileId, this._parentId);
		//this.modalContext?.submit({ temporaryFileId: this._temporaryFileId, parentId: this._parentId });
	}

	constructor() {
		super();
		this.#fileReader = new FileReader();
		this.#fileReader.onload = (e) => {
			if (typeof e.target?.result === 'string') {
				const fileContent = e.target.result;
				this.#dictionaryItemBuilder(fileContent);
			}
		};
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._parentId = this.data?.unique ?? undefined;
	}

	#dictionaryItemBuilder(htmlString: string) {
		const parser = new DOMParser();
		const doc = parser.parseFromString(htmlString, 'text/xml');
		const elements = doc.childNodes;

		this.#fileContent = this.#makeDictionaryItems(elements);
		this.requestUpdate();
	}

	#makeDictionaryItems(nodeList: NodeListOf<ChildNode>): Array<DictionaryItemPreview> {
		const items: Array<DictionaryItemPreview> = [];
		const list: Array<Element> = [];
		nodeList.forEach((node) => {
			if (node.nodeType === Node.ELEMENT_NODE && node.nodeName === 'DictionaryItem') {
				list.push(node as Element);
			}
		});

		list.forEach((item) => {
			items.push({
				name: item.getAttribute('Name') ?? '',
				children: this.#makeDictionaryItems(item.childNodes) ?? undefined,
			});
		});

		return items;
	}

	#onUpload(e: Event) {
		e.preventDefault();
		const formData = new FormData(this._form);
		const file = formData.get('file') as Blob;

		this.#fileReader.readAsText(file);
		this._temporaryFileId = file ? UmbId.new() : undefined;
	}

	#onParentChange(event: CustomEvent) {
		this._parentId = (event.target as UmbDictionaryItemInputElement).selectedIds[0] || undefined;
		//console.log((event.target as UmbDictionaryItemInputElement).selectedIds[0] || undefined);
	}

	async #onFileInput() {
		requestAnimationFrame(() => {
			this._form.requestSubmit();
		});
	}

	#onClear() {
		this._temporaryFileId = '';
	}

	render() {
		return html` <umb-body-layout headline=${this.localize.term('general_import')}>
			<uui-box>
				${when(
					this._temporaryFileId,
					() => this.#renderImportDestination(),
					() => this.#renderUploadZone(),
				)}
			</uui-box>
			<uui-button
				slot="actions"
				type="button"
				label=${this.localize.term('general_cancel')}
				@click=${this.#handleClose}></uui-button>
		</umb-body-layout>`;
	}

	#renderFileContents(items: Array<DictionaryItemPreview>): any {
		return html`${items.map((item: DictionaryItemPreview) => {
			return html`${item.name}
				<div>${this.#renderFileContents(item.children)}</div>`;
		})}`;
	}

	#renderImportDestination() {
		return html`
			<div id="wrapper">
				<div>
					<strong><umb-localize key="visuallyHiddenTexts_dictionaryItems">Dictionary items</umb-localize>:</strong>
					<div id="item-list">${this.#renderFileContents(this.#fileContent)}</div>
				</div>
				<div>
					<strong><umb-localize key="actions_chooseWhereToImport">Choose where to import</umb-localize>:</strong>
					Work in progress<br />
					${
						this._parentId
						// TODO
						// <umb-dictionary-item-input
						//	@change=${this.#onParentChange}
						//	.selectedIds=${this._parentId ? [this._parentId] : []}
						//	max="1">
						//	</umb-dictionary-item-input>
					}
				</div>

				${this.#renderNavigate()}
			</div>
		`;
	}

	#renderNavigate() {
		return html`<div id="nav">
			<uui-button label=${this.localize.term('general_import')} look="secondary" @click=${this.#onClear}>
				<uui-icon name="icon-arrow-left"></uui-icon>
				${this.localize.term('general_back')}
			</uui-button>
			<uui-button
				type="button"
				label=${this.localize.term('general_import')}
				look="primary"
				@click=${this.#submit}></uui-button>
		</div>`;
	}

	#renderUploadZone() {
		return html`<umb-localize key="dictionary_importDictionaryItemHelp"></umb-localize>
			<uui-form>
				<form id="form" name="form" @submit=${this.#onUpload}>
					<uui-form-layout-item>
						<uui-label for="file" slot="label" required>${this.localize.term('formFileUpload_pickFile')}</uui-label>
						<uui-input-file
							accept=".udt"
							name="file"
							id="file"
							@input=${this.#onFileInput}
							required
							required-message=${this.localize.term('formFileUpload_pickFile')}></uui-input-file>
					</uui-form-layout-item>
				</form>
			</uui-form>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
			}
			#item-list {
				padding: var(--uui-size-3) var(--uui-size-4);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
			}
			#item-list div {
				padding-left: 20px;
			}

			#wrapper {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-3);
			}
		`,
	];
}

export default UmbImportDictionaryModalLayout;

declare global {
	interface HTMLElementTagNameMap {
		'umb-import-dictionary-modal': UmbImportDictionaryModalLayout;
	}
}
