import type { UmbConfirmBulkActionModalEntityReferencesConfig } from '../../../global-components/types.js';
import type { UmbConfirmBulkActionModalEntityReferencesElement } from '../../../global-components/confirm-bulk-action-modal-entity-references.element.js';
import type {
	UmbBulkTrashWithRelationConfirmModalData,
	UmbBulkTrashWithRelationConfirmModalValue,
} from './bulk-trash-with-relation-modal.token.js';
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
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-bulk-trash-with-relation-confirm-modal')
export class UmbBulkTrashWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbBulkTrashWithRelationConfirmModalData,
	UmbBulkTrashWithRelationConfirmModalValue
> {
	@state()
	private _referencesConfig?: UmbConfirmBulkActionModalEntityReferencesConfig;

	// Three-state model for reference-aware trashing:
	//   undefined = loading (button disabled, no message yet)
	//   false     = blocked (button disabled, "cannot trash" message)
	//   true      = allowed (button enabled, normal confirmation message)
	@state()
	private _canTrash: boolean | undefined = undefined;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#initData();
	}

	async #initData() {
		if (!this.data) return;

		// If disableDeleteWhenReferenced is not set, allow trashing immediately.
		// Otherwise stay in loading state until the references component reports totals.
		if (!this.data.disableDeleteWhenReferenced) {
			this._canTrash = true;
		}

		this._referencesConfig = {
			uniques: this.data.uniques,
			itemRepositoryAlias: this.data.itemRepositoryAlias,
			referenceRepositoryAlias: this.data.referenceRepositoryAlias,
		};
	}

	#onReferencesChange(event: UmbChangeEvent) {
		event.stopPropagation();
		if (this._canTrash !== undefined) return;

		const target = event.target as UmbConfirmBulkActionModalEntityReferencesElement;
		const total = target.getTotalItems();
		this._canTrash = total === 0;
	}

	#renderMessage() {
		if (this._canTrash === undefined) return nothing;

		if (this._canTrash === false) {
			return html`<p>
				${unsafeHTML(this.localize.string('#defaultdialogs_cannotBulkTrashWhenReferenced', this.data?.uniques.length))}
			</p>`;
		}

		return html`<p>
			${unsafeHTML(this.localize.string('#defaultdialogs_confirmBulkTrash', this.data?.uniques.length))}
		</p>`;
	}

	override render() {
		const headline = this.localize.string('#actions_trash');

		return html`
			<uui-dialog-layout class="uui-text" headline=${headline}>
				${this.#renderMessage()}
				${this._referencesConfig
					? html`<umb-confirm-bulk-action-modal-entity-references
							.config=${this._referencesConfig}
							@change=${this.#onReferencesChange}></umb-confirm-bulk-action-modal-entity-references>`
					: nothing}

				<uui-button slot="actions" id="cancel" label=${this.localize.term('general_cancel')} @click=${this._rejectModal}></uui-button>

				<uui-button
					slot="actions"
					id="confirm"
					color="danger"
					look="primary"
					label=${this.localize.term('actions_trash')}
					?disabled=${!this._canTrash}
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

export { UmbBulkTrashWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-bulk-trash-with-relation-confirm-modal': UmbBulkTrashWithRelationConfirmModalElement;
	}
}
