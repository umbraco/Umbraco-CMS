import { UmbDocumentItemRepository } from '../../repository/index.js';
import type {
	UmbDocumentCreateOptionsModalData,
	UmbDocumentCreateOptionsModalValue,
} from './document-create-options-modal.token.js';
import { html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentTypeStructureRepository,
	type UmbAllowedDocumentTypeModel,
} from '@umbraco-cms/backoffice/document-type';

@customElement('umb-document-create-options-modal')
export class UmbDocumentCreateOptionsModalElement extends UmbModalBaseElement<
	UmbDocumentCreateOptionsModalData,
	UmbDocumentCreateOptionsModalValue
> {
	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);
	#documentItemRepository = new UmbDocumentItemRepository(this);

	@state()
	private _allowedDocumentTypes: UmbAllowedDocumentTypeModel[] = [];

	@state()
	private _headline: string = 'Create';

	async firstUpdated() {
		const parentUnique = this.data?.parent.unique;
		const documentTypeUnique = this.data?.documentType?.unique || null;

		this.#retrieveAllowedDocumentTypesOf(documentTypeUnique);

		if (parentUnique) {
			this.#retrieveHeadline(parentUnique);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	async #retrieveHeadline(parentUnique: string) {
		if (!parentUnique) return;
		const { data } = await this.#documentItemRepository.requestItems([parentUnique]);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = `Create at ${data[0].variants?.[0].name}`;
		}
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	render() {
		return html`
			<umb-body-layout headline=${this._headline ?? ''}>
				<uui-box>
					${this._allowedDocumentTypes.length === 0 ? html`<p>No allowed types</p>` : nothing}
					${this._allowedDocumentTypes.map(
						(documentType) => html`
							<uui-menu-item
								data-id=${ifDefined(documentType.unique)}
								href="${`section/content/workspace/document/create/parent/${this.data?.parent.entityType}/${
									this.data?.parent.unique ?? 'null'
								}/${documentType.unique}`}"
								label="${documentType.name}"
								@click=${this.#onNavigate}>
								> ${documentType.icon ? html`<umb-icon slot="icon" name=${documentType.icon}></umb-icon>` : nothing}
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbDocumentCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-create-options-modal': UmbDocumentCreateOptionsModalElement;
	}
}
