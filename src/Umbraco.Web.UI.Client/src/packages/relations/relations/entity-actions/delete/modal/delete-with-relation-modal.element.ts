import type { UmbConfirmActionModalEntityReferencesConfig } from '../../../global-components/types.js';
import type {
	UmbDeleteWithRelationConfirmModalData,
	UmbDeleteWithRelationConfirmModalValue,
} from './delete-with-relation-modal.token.js';
import {
	html,
	customElement,
	css,
	state,
	type PropertyValues,
	nothing,
	unsafeHTML,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-delete-with-relation-confirm-modal')
export class UmbDeleteWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbDeleteWithRelationConfirmModalData,
	UmbDeleteWithRelationConfirmModalValue
> {
	@state()
	private _name?: string;

	@state()
	private _referencesConfig?: UmbConfirmActionModalEntityReferencesConfig;

	@state()
	private _isDeletable?: boolean;

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
		this._isDeletable = item.isDeletable;

		this._referencesConfig = {
			unique: this.data.unique,
			itemRepositoryAlias: this.data.itemRepositoryAlias,
			referenceRepositoryAlias: this.data.referenceRepositoryAlias,
		};
	}

	override render() {
		const headline = this.localize.string('#actions_delete');
		const isNotDeletable = this._isDeletable === false;

		const message = isNotDeletable
			? html`<p>${this.localize.string('#defaultdialogs_cannotDeleteSystemItem')}</p>`
			: html`<p>${unsafeHTML(this.localize.string('#defaultdialogs_confirmdelete', this._name))}</p>`;

		const actions = isNotDeletable
			? html`
					<uui-button
						slot="actions"
						label=${this.localize.term('general_close')}
						look="primary"
						@click=${this._rejectModal}
						${umbFocus()}></uui-button>
				`
			: html`
					<uui-button
						slot="actions"
						id="cancel"
						label=${this.localize.term('general_cancel')}
						@click=${this._rejectModal}></uui-button>
					<uui-button
						slot="actions"
						id="confirm"
						color="danger"
						look="primary"
						label=${this.localize.term('general_delete')}
						@click=${this._submitModal}
						${umbFocus()}></uui-button>
				`;

		return html`
			<uui-dialog-layout class="uui-text" headline=${headline}>
				${message}
				${this._referencesConfig
					? html`<umb-confirm-action-modal-entity-references
							.config=${this._referencesConfig}></umb-confirm-action-modal-entity-references>`
					: nothing}
				${actions}
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

export { UmbDeleteWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-delete-with-relation-confirm-modal': UmbDeleteWithRelationConfirmModalElement;
	}
}
