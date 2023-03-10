import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { when } from 'lit-html/directives/when.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbTreeElement } from '../../../../shared/components/tree/tree.element';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository';
import { UmbImportDictionaryModalData, UmbImportDictionaryModalResult } from '.';
import { DictionaryUploadModel } from '@umbraco-cms/backend-api';
import { UmbModalBaseElement } from '@umbraco-cms/modal';

@customElement('umb-import-dictionary-modal-layout')
export class UmbImportDictionaryModalLayoutElement extends UmbModalBaseElement<
	UmbImportDictionaryModalData,
	UmbImportDictionaryModalResult
> {
	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];

	@query('#form')
	private _form!: HTMLFormElement;

	@state()
	private _uploadedDictionary?: DictionaryUploadModel;

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
		if (!this._uploadedDictionary?.fileName) return;

		this.modalHandler?.submit({
			fileName: this._uploadedDictionary.fileName,
			parentKey: this._selection[0],
		});
	}

	#handleClose() {
		this.modalHandler?.reject();
	}

	#submitForm() {
		this._form?.requestSubmit();
	}

	async #handleSubmit(e: SubmitEvent) {
		e.preventDefault();

		if (!this._form.checkValidity()) return;

		const formData = new FormData(this._form);
		const { data } = await this.#detailRepo.upload(formData);

		this._uploadedDictionary = data;

		if (!this._uploadedDictionary) {
			this._showErrorView = true;
			this._showImportView = false;
			return;
		}

		this._showErrorView = false;
		this._showUploadView = false;
		this._showImportView = true;
	}

	#handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = element.selection;
	}

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

	/// TODO => Tree view needs isolation and single-select option
	#renderImportView() {
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
				@selected=${this.#handleSelectionChange}
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

	// TODO => Determine what to display when dictionary import/upload fails
	#renderErrorView() {
		return html`Something went wrong`;
	}

	render() {
		return html` <umb-body-layout headline="Import">
			${when(this._showUploadView, () => this.#renderUploadView())}
			${when(this._showImportView, () => this.#renderImportView())}
			${when(this._showErrorView, () => this.#renderErrorView())}
		</umb-body-layout>`;
	}
}

export default UmbImportDictionaryModalLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-import-dictionary-modal-layout': UmbImportDictionaryModalLayoutElement;
	}
}
