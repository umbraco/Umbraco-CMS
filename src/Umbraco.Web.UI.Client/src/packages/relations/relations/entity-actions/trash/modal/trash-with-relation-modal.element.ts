import type {
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue,
} from './trash-with-relation-modal.token.js';
import { html, customElement, css, state, type PropertyValues, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

// import of local component
import '../../local-components/confirm-action-entity-references.element.js';

@customElement('umb-trash-with-relation-confirm-modal')
export class UmbTrashWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue
> {
	@state()
	_name?: string;

	@state()
	_referencesConfig?: any;

	#itemRepository?: UmbItemRepository<any>;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#initData();
	}

	async #initData() {
		if (!this.data) {
			this.#itemRepository?.destroy();
			return;
		}

		this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);

		const { data } = await this.#itemRepository.requestItems([this.data.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		this._name = item.name;

		this._referencesConfig = {
			unique: this.data.unique,
			itemRepositoryAlias: this.data.itemRepositoryAlias,
			referenceRepositoryAlias: this.data.referenceRepositoryAlias,
		};
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline="Trash">
				<p>Are you sure you want to move <strong>${this._name}</strong> to the recycle bin?</p>

				${this._referencesConfig
					? html`<umb-confirm-action-modal-entity-references
							.config=${this._referencesConfig}></umb-confirm-action-modal-entity-references>`
					: nothing}

				<uui-button slot="actions" id="cancel" label="Cancel" @click=${this._rejectModal}></uui-button>

				<uui-button
					slot="actions"
					id="confirm"
					color="danger"
					look="primary"
					label="Trash"
					@click=${this._submitModal}
					${umbFocus()}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}
		`,
	];
}

export { UmbTrashWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-trash-with-relation-confirm-modal': UmbTrashWithRelationConfirmModalElement;
	}
}
