import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UUIInputFileElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

interface DictionaryItem {
	name: string;
	children: Array<DictionaryItem>;
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

	@query('#file')
	private _fileInput!: UUIInputFileElement;

	#fileReader;

	#fileContent: Array<DictionaryItem> = [];

	#handleClose() {
		this.modalContext?.reject();
	}

	#submit() {
		this._form.requestSubmit();
	}

	#handleSubmit(e: Event) {
		e.preventDefault();
		const formData = new FormData(this._form);
		const file = formData.get('file') as File;

		this._temporaryFileId = file ? UmbId.new() : undefined;

		this.#fileReader.readAsText(file);

		//this.modalContext?.submit({ temporaryFileId: this._temporaryFileId, parentId: this._parentId });
		//this.modalContext?.submit();
	}

	async #onFileInput() {
		requestAnimationFrame(() => {
			this._form.requestSubmit();
		});
	}

	#onClear() {
		this._temporaryFileId = '';
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

	#dictionaryItemBuilder(htmlString: string) {
		const parser = new DOMParser();
		const doc = parser.parseFromString(htmlString, 'text/html');
		const elements = doc.body.childNodes;

		this.#fileContent = this.#makeDictionaryItems(elements);
		this.requestUpdate();
	}

	#makeDictionaryItems(nodeList: NodeListOf<ChildNode>): Array<DictionaryItem> {
		const items: Array<DictionaryItem> = [];
		const list: Array<Element> = [];
		nodeList.forEach((node) => {
			if (node.nodeType === Node.ELEMENT_NODE) {
				list.push(node as Element);
			}
		});

		list.forEach((item) => {
			items.push({
				name: item.getAttribute('name') ?? '',
				children: this.#makeDictionaryItems(item.childNodes) ?? undefined,
			});
		});
		return items;
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._parentId = this.data?.unique ?? undefined;
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

	#renderFileContents(items: Array<DictionaryItem>): any {
		return html`${items.map((item: DictionaryItem) => {
			return html`${item.name}
				<div>${this.#renderFileContents(item.children)}</div>`;
		})}`;
	}

	#renderImportDestination() {
		return html`
			<div>
				<strong><umb-localize key="visuallyHiddenTexts_dictionaryItems">Dictionary items</umb-localize>:</strong>

				<div id="item-list">${this.#renderFileContents(this.#fileContent)}</div>
			</div>
			<div>
				<strong><umb-localize key="actions_chooseWhereToImport">Choose where to import</umb-localize>:</strong>

				<umb-tree alias="Umb.Tree.Dictionary"></umb-tree>
			</div>

			${this.#renderNavigate()}
		`;
	}

	#renderNavigate() {
		return html`<div>
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
				<form id="form" name="form" @submit=${this.#handleSubmit}>
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
		`,
	];
}

export default UmbImportDictionaryModalLayout;

declare global {
	interface HTMLElementTagNameMap {
		'umb-import-dictionary-modal': UmbImportDictionaryModalLayout;
	}
}
