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

	@state()
	private _allowedDocumentTypes: UmbAllowedDocumentTypeModel[] = [];

	@state()
	private _headline: string = 'Create';

	async firstUpdated() {
		const documentUnique = this.data?.unique || null;

		this.#retrieveAllowedDocumentTypesOf(documentUnique);

		if (documentUnique) {
			this.#retrieveHeadline(documentUnique);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.allowedDocumentTypesOf(unique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	async #retrieveHeadline(unique: string) {
		if (!unique) return;
		const { data } = await this.#documentItemRepository.requestItems(id);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = `Create at ${data.variants?.[0].name}`;
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
						(item) => html`
							<uui-menu-item data-id=${ifDefined(item.unique)} href="" label="${item.name}">
								${item.icon ? html`<uui-icon slot="icon" name=${item.icon}></uui-icon>` : nothing}
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
