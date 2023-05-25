import { UmbDocumentTypeRepository } from '../../repository/document-type.repository.js';
import { html, nothing , customElement, state , ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbAllowedDocumentTypesModalData, UmbAllowedDocumentTypesModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-allowed-document-types-modal')
export class UmbAllowedDocumentTypesModalElement extends UmbModalBaseElement<
	UmbAllowedDocumentTypesModalData,
	UmbAllowedDocumentTypesModalResult
> {
	#documentTypeRepository = new UmbDocumentTypeRepository(this);

	@state()
	private _allowedDocumentTypes: DocumentTypeTreeItemResponseModel[] = [];

	async firstUpdated() {
		// TODO: Support root aka. id of null? or maybe its an active prop, like 'atRoot'.
		// TODO: show error
		if (!this.data?.id) return;

		const { data } = await this.#documentTypeRepository.requestAllowedChildTypesOf(this.data.id);

		if (data) {
			this._allowedDocumentTypes = data;
		}
	}

	private _handleCancel() {
		this.modalHandler?.reject();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentTypeKey = target.dataset.id;
		if (!documentTypeKey) throw new Error('No document type id found');
		this.modalHandler?.submit({ documentTypeKey });
	}

	render() {
		return html`
			<umb-body-layout headline="Headline">
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
