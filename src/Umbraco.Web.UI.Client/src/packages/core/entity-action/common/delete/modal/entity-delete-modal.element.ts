import type { UmbEntityDeleteModalData, UmbEntityDeleteModalValue } from './entity-delete-modal.token.js';
import { html, customElement, css, state, type PropertyValues, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-delete-modal')
export class UmbEntityDeleteModalElement<
	DataType extends UmbEntityDeleteModalData = UmbEntityDeleteModalData,
> extends UmbModalBaseElement<DataType, UmbEntityDeleteModalValue> {
	@state()
	protected _name?: string;

	@state()
	protected _isDeletable?: boolean;

	protected _itemRepository?: UmbItemRepository<any>;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this._initData();
	}

	protected async _initData() {
		if (!this.data) {
			this._itemRepository?.destroy();
			return;
		}

		if (!this.data.unique) throw new Error('Cannot delete without a unique identifier.');

		this._itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);

		const { data } = await this._itemRepository.requestItems([this.data.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		this._name = item.name;
		this._isDeletable = item.isDeletable;
	}

	override render() {
		const headline = this.data?.headline
			? this.localize.string(this.data.headline)
			: this.localize.string('#actions_delete');

		const isNotDeletable = this._isDeletable === false;

		const message = isNotDeletable
			? html`<p>${this.localize.string('#defaultdialogs_cannotDeleteSystemItem')}</p>`
			: html`<p>
					${unsafeHTML(this.localize.string(this.data?.message ?? '#defaultdialogs_confirmdelete', this._name))}
				</p>`;

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
						label=${this.localize.term('general_cancel')}
						@click=${this._rejectModal}></uui-button>
					<uui-button
						slot="actions"
						color="danger"
						look="primary"
						label=${this.localize.term('general_delete')}
						@click=${this._submitModal}
						${umbFocus()}></uui-button>
				`;

		return html` <uui-dialog-layout class="uui-text" headline=${headline}> ${message} ${actions} </uui-dialog-layout> `;
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

export { UmbEntityDeleteModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-delete-modal': UmbEntityDeleteModalElement;
	}
}
