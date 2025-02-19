import type {
	UmbBulkDeleteWithRelationConfirmModalData,
	UmbBulkDeleteWithRelationConfirmModalValue,
} from './bulk-delete-with-relation-modal.token.js';
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

// import of local component
import '../../local-components/confirm-bulk-action-entity-references.element.js';

@customElement('umb-bulk-delete-with-relation-confirm-modal')
export class UmbBulkDeleteWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbBulkDeleteWithRelationConfirmModalData,
	UmbBulkDeleteWithRelationConfirmModalValue
> {
	@state()
	_referencesConfig?: any;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#initData();
	}

	async #initData() {
		if (!this.data) return;

		this._referencesConfig = {
			uniques: this.data.uniques,
			itemRepositoryAlias: this.data.itemRepositoryAlias,
			referenceRepositoryAlias: this.data.referenceRepositoryAlias,
		};
	}

	override render() {
		const headline = this.localize.string('#actions_delete');
		const message = '#defaultdialogs_confirmBulkDelete';

		return html`
			<uui-dialog-layout class="uui-text" headline=${headline}>
				<p>${unsafeHTML(this.localize.string(message, this.data?.uniques.length))}</p>
				${this._referencesConfig
					? html`<umb-confirm-bulk-action-modal-entity-references
							.config=${this._referencesConfig}></umb-confirm-bulk-action-modal-entity-references>`
					: nothing}

				<uui-button slot="actions" id="cancel" label="Cancel" @click=${this._rejectModal}></uui-button>

				<uui-button
					slot="actions"
					id="confirm"
					color="danger"
					look="primary"
					label=${this.localize.term('actions_delete')}
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

export { UmbBulkDeleteWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-delete-with-relation-confirm-modal': UmbBulkDeleteWithRelationConfirmModalElement;
	}
}
