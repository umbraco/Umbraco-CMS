import { UmbDictionaryImportRepository } from '../../repository/index.js';
import type { UmbImportDictionaryModalData, UmbImportDictionaryModalValue } from './import-dictionary-modal.token.js';
import { css, html, customElement, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';
import { TemporaryFileStatus, UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

interface UmbDictionaryItemPreview {
	id: string;
	name: string;
	children: Array<UmbDictionaryItemPreview>;
}

@customElement('umb-import-dictionary-modal')
export class UmbImportDictionaryModalLayout extends UmbModalBaseElement<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalValue
> {
	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	private _parentUnique: string | null = null;

	@state()
	private _temporaryFileId = '';

	@state()
	private _importButtonState?: UUIButtonState;

	@state()
	private _importAllowed = false;

	@query('#form')
	private _form!: HTMLFormElement;

	#fileReader;
	#fileContent: Array<UmbDictionaryItemPreview> = [];
	#dictionaryImportRepository = new UmbDictionaryImportRepository(this);
	#temporaryFileManager = new UmbTemporaryFileManager(this);
	#abortController?: AbortController;

	constructor() {
		super();

		this.#fileReader = new FileReader();

		this.#fileReader.onload = (e) => {
			if (typeof e.target?.result === 'string') {
				const fileContent = e.target.result;
				this.#dictionaryPreviewBuilder(fileContent);
			}
		};
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this._parentUnique = this.data?.unique ?? null;
		this._selectionConfiguration.selection = this._parentUnique ? [this._parentUnique] : [];
	}

	async #submit() {
		const { error } = await this.#dictionaryImportRepository.requestImport(this._temporaryFileId, this._parentUnique);
		if (error) return;

		this._submitModal();
	}

	#dictionaryPreviewBuilder(htmlString: string) {
		const parser = new DOMParser();
		const doc = parser.parseFromString(htmlString, 'text/xml');
		const elements = doc.childNodes;

		this.#fileContent = this.#dictionaryPreviewItemBuilder(elements);
		this.requestUpdate();
	}

	#dictionaryPreviewItemBuilder(nodeList: NodeListOf<ChildNode>): Array<UmbDictionaryItemPreview> {
		const items: Array<UmbDictionaryItemPreview> = [];
		const list: Array<Element> = [];
		nodeList.forEach((node) => {
			if (node.nodeType === Node.ELEMENT_NODE && node.nodeName === 'DictionaryItem') {
				list.push(node as Element);
			}
		});

		list.forEach((item) => {
			items.push({
				name: item.getAttribute('Name') ?? '',
				id: item.getAttribute('Key') ?? '',
				children: this.#dictionaryPreviewItemBuilder(item.childNodes) ?? undefined,
			});
		});

		return items;
	}

	async #onUpload(e: Event) {
		e.preventDefault();

		this.#onClear();

		const formData = new FormData(this._form);
		const file = formData.get('file') as File;
		if (!file) throw new Error('File is missing');

		this.#fileReader.readAsText(file);
		const temporaryUnique = UmbId.new();

		this.#abortController = new AbortController();

		const { status } = await this.#temporaryFileManager.uploadOne({
			file,
			temporaryUnique,
			abortController: this.#abortController,
		});

		if (status === TemporaryFileStatus.SUCCESS) {
			this._importAllowed = true;
			this._temporaryFileId = temporaryUnique;
		}
	}

	async #onFileInput() {
		requestAnimationFrame(() => {
			this._form.requestSubmit();
		});
	}

	#onClear() {
		this._temporaryFileId = '';
		this._importAllowed = false;
		this.#abortController?.abort();
		this.#abortController = undefined;
	}

	override render() {
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
				@click=${this._rejectModal}></uui-button>
		</umb-body-layout>`;
	}

	#renderFileContents(items: Array<UmbDictionaryItemPreview>): any {
		return html`${items.map((item: UmbDictionaryItemPreview) => {
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
				.state=${this._importButtonState}
				?disabled=${!this._importAllowed}
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
						<uui-label for="file" slot="label" required>${this.localize.term('dictionary_pickFile')}</uui-label>
						<uui-input-file
							accept=".udt"
							name="file"
							id="file"
							@input=${this.#onFileInput}
							required
							required-message=${this.localize.term('dictionary_pickFileRequired')}></uui-input-file>
					</uui-form-layout-item>
				</form>
			</uui-form>`;
	}

	static override styles = [
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
