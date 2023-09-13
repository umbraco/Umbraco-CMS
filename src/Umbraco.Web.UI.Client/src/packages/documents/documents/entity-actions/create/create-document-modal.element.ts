import { html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCreateDocumentModalData, UmbCreateDocumentModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';

@customElement('umb-create-document-modal')
export class UmbCreateDocumentModalElement extends UmbModalBaseElement<
	UmbCreateDocumentModalData,
	UmbCreateDocumentModalResult
> {
	#documentRepository = new UmbDocumentRepository(this);

	@state()
	private _allowedDocumentTypes: DocumentTypeTreeItemResponseModel[] = [];

	@state()
	private _headline?: string;

	async firstUpdated() {
		await this.#retrieveAllowedChildrenOf(this.data?.id || null);
	}

	async #retrieveAllowedChildrenOf(id: string | null) {
		const { data } = await this.#documentRepository.requestAllowedDocumentTypesOf(id);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const target = event.target as HTMLButtonElement;
		const documentTypeId = target.dataset.id;
		if (!documentTypeId) throw new Error('No document type id found');
		this.modalContext?.submit({ documentTypeId });
	}

	render() {
		return html`
			<umb-body-layout headline=${this._headline ?? ''}>
				<uui-box>
					${this._allowedDocumentTypes.length === 0 ? html`<p>No allowed types</p>` : nothing}
					${this._allowedDocumentTypes.map(
						(item) => html`
							<uui-menu-item data-id=${ifDefined(item.id)} @click=${this.#onClick} label="${ifDefined(item.name)}">
								${item.icon ? html`<uui-icon slot="icon" name=${item.icon}></uui-icon>` : nothing}
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbCreateDocumentModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-modal': UmbCreateDocumentModalElement;
	}
}
