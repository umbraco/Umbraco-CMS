import { html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbAllowedDocumentTypesModalData, UmbAllowedDocumentTypesModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';

@customElement('umb-allowed-document-types-modal')
export class UmbAllowedDocumentTypesModalElement extends UmbModalBaseElement<
	UmbAllowedDocumentTypesModalData,
	UmbAllowedDocumentTypesModalResult
> {
	#documentRepository = new UmbDocumentRepository(this);

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
		const { data } = await this.#documentRepository.requestAllowedDocumentTypesOf(id);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	private async _retrieveAllowedChildrenOfRoot() {
		const { data } = await this.#documentRepository.requestAllowedDocumentTypesAtRoot();

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
		const documentTypeKey = target.dataset.id;
		if (!documentTypeKey) throw new Error('No document type id found');
		this.modalContext?.submit({ documentTypeKey });
	}

	render() {
		return html`
			<umb-body-layout headline=${this._headline ?? ''}>
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

	static styles = [UmbTextStyles];
}

export default UmbAllowedDocumentTypesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-allowed-document-types-modal': UmbAllowedDocumentTypesModalElement;
	}
}
