import { UmbDocumentTypeDetailRepository } from '../../repository/index.js';
import type {
	UmbDocumentTypePropertyPickerModalData,
	UmbDocumentTypePropertyPickerModalValue,
} from './document-type-property-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-document-type-property-picker-modal')
export class UmbDocumentTypePropertyPickerModalElement extends UmbModalBaseElement<
	UmbDocumentTypePropertyPickerModalData,
	UmbDocumentTypePropertyPickerModalValue
> {
	@state()
	private _properties: Array<UmbPropertyTypeModel> = [];

	#detailRepository = new UmbDocumentTypeDetailRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => {
			this.value = { selection };
		});
	}

	#onItemSelected(event: CustomEvent, item: UmbPropertyTypeModel) {
		event.stopPropagation();
		this.#selectionManager.select(item.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(item.unique));
	}

	#onItemDeselected(event: CustomEvent, item: UmbPropertyTypeModel) {
		event.stopPropagation();
		this.#selectionManager.deselect(item.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(item.unique));
	}

	override async firstUpdated() {
		if (!this.data?.documentType?.unique) {
			throw new Error('Document type unique is required');
		}

		const { data } = await this.#detailRepository.requestByUnique(this.data.documentType.unique);
		this._properties = data?.properties ?? [];
	}

	get #filteredProperties() {
		if (this.data?.filter) {
			return this._properties.filter(this.data.filter);
		} else {
			return this._properties;
		}
	}

	override render() {
		return html`<umb-body-layout headline="Select Properties">
			<uui-box>
				${this.#filteredProperties.length > 0
					? repeat(
							this.#filteredProperties,
							(item) => item.unique,
							(item) => html`
								<uui-ref-node
									name=${item.name ?? ''}
									detail=${item.alias ?? ''}
									selectable
									select-only
									@selected=${(event: CustomEvent) => this.#onItemSelected(event, item)}
									@deselected=${(event: CustomEvent) => this.#onItemDeselected(event, item)}
									?selected=${this.value.selection.includes(item.unique)}>
									<uui-icon slot="icon" name="icon-settings"></uui-icon>
								</uui-ref-node>
							`,
						)
					: html`There are no properties to choose from.`}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this._rejectModal}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this._submitModal}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export { UmbDocumentTypePropertyPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-property-picker-modal': UmbDocumentTypePropertyPickerModalElement;
	}
}
