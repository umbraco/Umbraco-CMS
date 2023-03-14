import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbDocumentTypeRepository } from '../../repository/document-type.repository';
import { UmbAllowedDocumentTypesModalData, UmbAllowedDocumentTypesModalResult } from '.';
import { UmbModalBaseElement } from '@umbraco-cms/modal';
import { DocumentTypeTreeItemModel } from '@umbraco-cms/backend-api';

@customElement('umb-allowed-document-types-modal')
export class UmbAllowedDocumentTypesModalElement extends UmbModalBaseElement<
	UmbAllowedDocumentTypesModalData,
	UmbAllowedDocumentTypesModalResult
> {
	static styles = [UUITextStyles];

	#documentTypeRepository = new UmbDocumentTypeRepository(this);

	@state()
	private _allowedDocumentTypes: DocumentTypeTreeItemModel[] = [];

	async firstUpdated() {
		// TODO: show error
		if (!this.data?.key) return;

		const { data } = await this.#documentTypeRepository.requestAllowedChildTypesOf(this.data.key);

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
		const documentTypeKey = target.value;
		this.modalHandler?.submit({ documentTypeKey });
	}

	render() {
		return html`
			<umb-body-layout headline="Headline">
				<div>Render list of create options for ${this.data?.key}</div>

				<ul>
					${this._allowedDocumentTypes.map(
						(item) =>
							html`
								<li>
									<button type="button" value=${ifDefined(item.key)} @click=${this.#onClick}>${item.name}</button>
								</li>
							`
					)}
				</ul>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._handleCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbAllowedDocumentTypesModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-allowed-document-types-modal': UmbAllowedDocumentTypesModalElement;
	}
}
