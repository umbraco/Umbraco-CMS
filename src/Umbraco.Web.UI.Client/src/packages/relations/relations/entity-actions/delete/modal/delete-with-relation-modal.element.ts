import type { UmbConfirmActionModalEntityReferencesConfig } from '../../../global-components/types.js';
import type { UmbDeleteWithRelationConfirmModalData } from './delete-with-relation-modal.token.js';
import { UmbEntityDeleteModalElement } from '@umbraco-cms/backoffice/entity-action';
import { html, customElement, state, nothing, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-delete-with-relation-confirm-modal')
export class UmbDeleteWithRelationConfirmModalElement extends UmbEntityDeleteModalElement<UmbDeleteWithRelationConfirmModalData> {
	@state()
	private _referencesConfig?: UmbConfirmActionModalEntityReferencesConfig;

	protected override async _initData() {
		await super._initData();

		if (!this.data?.unique) return;

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
				${this._referencesConfig && !isNotDeletable
					? html`<umb-confirm-action-modal-entity-references
							.config=${this._referencesConfig}></umb-confirm-action-modal-entity-references>`
					: nothing}
				${actions}
			</uui-dialog-layout>
		`;
	}
}

export { UmbDeleteWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-delete-with-relation-confirm-modal': UmbDeleteWithRelationConfirmModalElement;
	}
}
