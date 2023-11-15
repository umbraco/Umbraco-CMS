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

	#handleClose() {
		this.modalContext?.reject();
	}

	#submit() {
		this._form.requestSubmit();
	}

	#handleSubmit(e: Event) {
		e.preventDefault();
		const formData = new FormData(this._form);
		const file = formData.get('file');

		this._temporaryFileId = file ? UmbId.new() : undefined;

		console.log(this._temporaryFileId, file);

		//this.modalContext?.submit({ temporaryFileId: this._temporaryFileId, parentId: this._parentId });
		//this.modalContext?.submit();
	}

	#onFileInput() {
		this._form.requestSubmit();
	}

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._parentId = this.data?.unique ?? undefined;
	}

	render() {
		return html` <umb-body-layout headline=${this.localize.term('general_import')}>
			<uui-box>
				<umb-localize key="dictionary_importDictionaryItemHelp"></umb-localize>

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
			<uui-button
				slot="actions"
				type="button"
				label=${this.localize.term('general_import')}
				look="primary"
				@click=${this.#submit}></uui-button>
		</umb-body-layout>`;
	}

	#renderImportDestination() {
		return html`<umb-localize key="actions_chooseWhereToImport">c</umb-localize>Help`;
	}

	#renderUploadZone() {
		return html`<uui-form>
			<form id="form" name="form" @submit=${this.#handleSubmit}>
				<uui-form-layout-item>
					<uui-label for="file" slot="label" required>${this.localize.term('formFileUpload_pickFile')}</uui-label>
					<uui-input-file
						accept=".udt"
						name="file"
						id="file"
						@change=${this.#onFileInput}
						required
						required-message=${this.localize.term('formFileUpload_pickFile')}></uui-input-file>
				</uui-form-layout-item>
			</form>
		</uui-form>`;
	}

	/*
	@query('#form')
	private _form!: HTMLFormElement;

	@state()
	private _uploadedDictionaryTempId?: string;

	@state()
	private _showUploadView = true;

	@state()
	private _showImportView = false;

	@state()
	private _showErrorView = false;

	@state()
	private _selection: Array<string> = [];

	#detailRepo = new UmbDictionaryRepository(this);

	async #importDictionary() {
		if (!this._uploadedDictionaryTempId) return;

		this.modalContext?.submit({
			temporaryFileId: this._uploadedDictionaryTempId,
			parentId: this._selection[0],
		});
	}

	#handleClose() {
		this.modalContext?.reject();
	}

	#submitForm() {
		this._form?.requestSubmit();
	}

	async #handleSubmit(e: SubmitEvent) {
		e.preventDefault();

		if (!this._form.checkValidity()) return;

		const formData = new FormData(this._form);

		const uploadData: ImportDictionaryRequestModel = {
			temporaryFileId: formData.get('file')?.toString() ?? '',
		};

		// TODO: fix this upload experience. We need to update our form so it gets temporary file id from the server:
		const { data } = await this.#detailRepo.upload(uploadData);

		if (!data) return;

		this._uploadedDictionaryTempId = data;
		// TODO: We need to find another way to gather the data of the uploaded dictionary, to represent the dictionaryItems? See further below.
		//this._uploadedDictionary = data;

		if (!this._uploadedDictionaryTempId) {
			this._showErrorView = true;
			this._showImportView = false;
			return;
		}

		this._showErrorView = false;
		this._showUploadView = false;
		this._showImportView = true;
	}

	/*
	#handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = element.selection;
	}
	*/
	/*
	#renderUploadView() {
		return html`<p>
				To import a dictionary item, find the ".udt" file on your computer by clicking the "Import" button (you'll be
				asked for confirmation on the next screen)
			</p>
			<uui-form>
				<form id="form" name="form" @submit=${this.#handleSubmit}>
					<uui-form-layout-item>
						<uui-label for="file" slot="label" required>File</uui-label>
						<div>
							<uui-input-file
								accept=".udt"
								name="file"
								id="file"
								required
								required-message="File is required"></uui-input-file>
						</div>
					</uui-form-layout-item>
				</form>
			</uui-form>
			<uui-button slot="actions" type="button" label="Cancel" @click=${this.#handleClose}></uui-button>
			<uui-button slot="actions" type="button" label="Import" look="primary" @click=${this.#submitForm}></uui-button>`;
	}
	*/

	/// TODO => Tree view needs isolation and single-select option
	/*#renderImportView() {
		//TODO: gather this data in some other way, we cannot use the feedback from the server anymore. can we use info about the file directly? or is a change to the end point required?
		/*
		if (!this._uploadedDictionary?.dictionaryItems) return;

		return html`
			<b>Dictionary items</b>
			<ul>
				${repeat(
					this._uploadedDictionary.dictionaryItems,
					(item) => item.name,
					(item) => html`<li>${item.name}</li>`
				)}
			</ul>
			<hr />
			<b>Choose where to import dictionary items (optional)</b>
			<umb-tree
				alias="Umb.Tree.Dictionary"
				@selection-change=${this.#handleSelectionChange}
				.selection=${this._selection}
				selectable></umb-tree>

			<uui-button slot="actions" type="button" label="Cancel" @click=${this.#handleClose}></uui-button>
			<uui-button
				slot="actions"
				type="button"
				label="Import"
				look="primary"
				@click=${this.#importDictionary}></uui-button>
		`;
	}
	*/

	// TODO => Determine what to display when dictionary import/upload fails
	/*#renderErrorView() {
		return html`Something went wrong`;
	}*/
	/*
	render() {
		return html` <umb-body-layout headline="Import">
			${when(this._showUploadView, () => this.#renderUploadView())}
			${when(this._showImportView, () => this.#renderImportView())}
			${when(this._showErrorView, () => this.#renderErrorView())}
		</umb-body-layout>`;
	}*/

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
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
