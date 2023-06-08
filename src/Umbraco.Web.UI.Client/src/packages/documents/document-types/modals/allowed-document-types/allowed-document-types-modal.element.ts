import { UmbDocumentTypeRepository } from '../../repository/document-type.repository.js';
import { html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbAllowedDocumentTypesModalData, UmbAllowedDocumentTypesModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { DocumentTypeResponseModel, DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-allowed-document-types-modal')
export class UmbAllowedDocumentTypesModalElement extends UmbModalBaseElement<
	UmbAllowedDocumentTypesModalData,
	UmbAllowedDocumentTypesModalResult
> {
	#documentTypeRepository = new UmbDocumentTypeRepository(this);

	@state()
	private _allowedDocumentTypes: DocumentTypeTreeItemResponseModel[] = [];

	@state()
	private _headline?: string;

	public connectedCallback() {
		super.connectedCallback();

		const parentId = this.data?.parentId;
		const parentName = this.data?.parentName;
		if (parentName) {
			this._headline = `Create at '${parentName}'`;
		} else {
			this._headline = `Create`;
		}
		if (parentId) {
			// TODO: Support root aka. id of null? or maybe its an active prop, like 'atRoot'.
			// TODO: show error

			this._retrieveAllowedChildrenOf(parentId);
		} else {
			this._retrieveAllowedChildrenOfRoot();
		}
	}

	private async _retrieveAllowedChildrenOf(id: string) {
		const { data } = await this.#documentTypeRepository.requestAllowedChildTypesOf(id);

		if (data) {
			this._allowedDocumentTypes = data;
		}
	}

	private async _retrieveAllowedChildrenOfRoot() {
		// TODO: This is a hack until we get the right end points (Which will become a Document end point. meaning this modal should have another name, it should be named so its clear that this is for documents, not document types. ex.: 'create-document-modal')
		const { data } = await this.#documentTypeRepository.requestRootTreeItems();
		if (!data) return;

		const allFullModels: Array<DocumentTypeResponseModel & {$type: ''}> = [];
		await Promise.all(
			data.items.map((item) => {
				if (item.id) {
					return this.#documentTypeRepository.requestById(item.id).then((result) => {if(result.data) {allFullModels.push({$type: '', ...result.data})}});
				}
				return Promise.resolve();
			})
		);

		this._allowedDocumentTypes = allFullModels.filter((item) => item.allowedAsRoot) ?? [];
		// End of hack...^^
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentTypeKey = target.dataset.id;
		if (!documentTypeKey) throw new Error('No document type id found');
		this.modalContext?.submit({ documentTypeKey });
	}

	render() {
		return html`
			<umb-body-layout headline=${this._headline}>
				<uui-box>
					${this._allowedDocumentTypes.length === 0 ? html`<p>No allowed types</p>` : nothing}
					${this._allowedDocumentTypes.map(
						(item) =>
							html`
								<uui-menu-item data-id=${ifDefined(item.id)} @click=${this.#onClick} label="${ifDefined(item.name)}">
									${item.icon ? html`<uui-icon slot="icon" name=${item.icon}></uui-icon>` : nothing}
								</uui-menu-item>
							`
					)}
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UUITextStyles];
}

export default UmbAllowedDocumentTypesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-allowed-document-types-modal': UmbAllowedDocumentTypesModalElement;
	}
}
