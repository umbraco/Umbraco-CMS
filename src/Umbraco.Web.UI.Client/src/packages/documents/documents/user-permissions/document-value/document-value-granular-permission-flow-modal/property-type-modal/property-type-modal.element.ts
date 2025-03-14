import type {
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalData,
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalValue,
} from './property-type-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UMB_ENTITY_USER_PERMISSION_MODAL } from '@umbraco-cms/backoffice/user-permission';

@customElement('umb-document-value-granular-user-permission-flow-property-type-modal')
export class UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalElement extends UmbModalBaseElement<
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalData,
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalValue
> {
	@state()
	private _documentTypeProperties: Array<UmbPropertyTypeModel> = [];

	@state()
	private _documentTypeName?: string;

	@state()
	_selectedItem: UmbPropertyTypeModel | null = null;

	#detailRepository = new UmbDocumentTypeDetailRepository(this);

	#onItemSelected(event: CustomEvent, item: UmbPropertyTypeModel) {
		event.stopPropagation();
		this._selectedItem = item;
	}

	#onItemDeselected(event: CustomEvent) {
		event.stopPropagation();
		this._selectedItem = null;
	}

	override async firstUpdated() {
		if (!this.data?.documentType?.unique) {
			throw new Error('Document type unique is required');
		}

		const { data } = await this.#detailRepository.requestByUnique(this.data.documentType.unique);
		this._documentTypeProperties = data?.properties ?? [];
		this._documentTypeName = data?.name;
	}

	#getItemDetail(item: UmbPropertyTypeModel): string {
		const isMandatory = item.validation?.mandatory ? ' - Mandatory' : '';
		const variesByCulture = item.variesByCulture ? ' - Varies by culture' : '';
		const variesBySegment = item.variesBySegment ? ' - Varies by segment' : '';
		return `${item.alias} ${isMandatory} ${variesByCulture} ${variesBySegment}`;
	}

	#next() {
		if (!this._selectedItem) {
			throw new Error('Could not proceed, no property was selected');
		}

		this.#selectEntityUserPermissionsForProperty();
	}

	async #selectEntityUserPermissionsForProperty() {
		if (!this._selectedItem) {
			throw new Error('Could not open permissions modal, no property was provided');
		}

		const headline = `Permissions for ${this._documentTypeName}: ${this._selectedItem.name}`;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Could not open permissions modal, modal manager is not available');
		}

		const modal = modalManager.open(this, UMB_ENTITY_USER_PERMISSION_MODAL, {
			data: {
				entityType: 'document-value',
				headline,
			},
		});

		try {
			const value = await modal.onSubmit();
			this.updateValue({
				propertyType: { unique: this._selectedItem.unique },
				verbs: value.allowedVerbs,
			});
			this._submitModal();
		} catch (error) {
			console.log(error);
		}
	}

	override render() {
		return html`<umb-body-layout headline="Choose Property">
			<uui-box>
				${this._documentTypeProperties.length > 0
					? repeat(
							this._documentTypeProperties,
							(item) => item.unique,
							(item) => html`
								<uui-ref-node
									name=${item.name ?? ''}
									detail=${this.#getItemDetail(item)}
									selectable
									select-only
									@selected=${(event: CustomEvent) => this.#onItemSelected(event, item)}
									@deselected=${(event: CustomEvent) => this.#onItemDeselected(event)}
									?selected=${this._selectedItem?.unique === item.unique}>
									<uui-icon slot="icon" name="icon-settings"></uui-icon>
								</uui-ref-node>
							`,
						)
					: html`There are no properties to choose from.`}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this._rejectModal}></uui-button>
				<uui-button
					label="${this.localize.term('general_next')}"
					look="primary"
					color="positive"
					@click=${this.#next}
					?disabled=${!this._selectedItem}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export { UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-value-granular-user-permission-flow-property-type-modal': UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalElement;
	}
}
