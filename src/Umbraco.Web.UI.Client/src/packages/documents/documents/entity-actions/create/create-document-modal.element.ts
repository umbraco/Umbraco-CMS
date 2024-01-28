import { UmbDocumentItemRepository } from '../../repository/index.js';
import { html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbCreateDocumentModalData, UmbCreateDocumentModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentTypeStructureRepository,
	type UmbAllowedDocumentTypeModel,
} from '@umbraco-cms/backoffice/document-type';

@customElement('umb-create-document-modal')
export class UmbCreateDocumentModalElement extends UmbModalBaseElement<
	UmbCreateDocumentModalData,
	UmbCreateDocumentModalValue
> {
	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);
	#documentItemRepository = new UmbDocumentItemRepository(this);

	@state()
	private _allowedDocumentTypes: UmbAllowedDocumentTypeModel[] = [];

	@state()
	private _headline: string = 'Create';

	async firstUpdated() {
		const documentUnique = this.data?.document?.unique || null;
		const documentTypeUnique = this.data?.documentType?.unique || null;

		this.#retrieveAllowedDocumentTypesOf(documentTypeUnique);

		if (documentUnique) {
			this.#retrieveHeadline(documentUnique);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	async #retrieveHeadline(unique: string) {
		if (!unique) return;
		const { data } = await this.#documentItemRepository.requestItems([unique]);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = `Create at ${data[0].variants?.[0].name}`;
		}
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
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
								href="${`section/content/workspace/document/create/${this.data?.unique ?? 'null'}/${
									documentType.unique
								}`}"
								label="${documentType.name}">
								${documentType.icon ? html`<uui-icon slot="icon" name=${documentType.icon}></uui-icon>` : nothing}
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
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
