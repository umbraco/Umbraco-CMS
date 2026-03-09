import type {
	UmbConfirmActionModalEntityReferencesConfig,
	UmbConfirmActionModalEntityReferencesElement,
} from '../../../global-components/types.js';
import type {
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue,
} from './trash-with-relation-modal.token.js';
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
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-trash-with-relation-confirm-modal')
export class UmbTrashWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue
> {
	@state()
	private _name?: string;

	@state()
	private _referencesConfig?: UmbConfirmActionModalEntityReferencesConfig;

	// Three-state model for reference-aware trashing:
	//   undefined = loading (button disabled, no message yet)
	//   false     = blocked (button disabled, "cannot trash" message)
	//   true      = allowed (button enabled, normal confirmation message)
	@state()
	private _canTrash: boolean | undefined = undefined;

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

		if (this.data.itemDataResolver) {
			const resolver = new this.data.itemDataResolver(this);
			resolver.setData(item);
			this._name = await resolver.getName();
		} else {
			this._name = item.name;
		}

		// If disableDeleteWhenReferenced is not set, allow trashing immediately.
		// Otherwise stay in loading state until the references component reports totals.
		if (!this.data.disableDeleteWhenReferenced) {
			this._canTrash = true;
		}

		this._referencesConfig = {
			unique: this.data.unique,
			itemRepositoryAlias: this.data.itemRepositoryAlias,
			referenceRepositoryAlias: this.data.referenceRepositoryAlias,
		};
	}

	#onReferencesChange(event: UmbChangeEvent) {
		event.stopPropagation();
		if (this._canTrash !== undefined) return;
		const target = event.target as UmbConfirmActionModalEntityReferencesElement;
		const total = target.getTotalReferencedBy() + target.getTotalDescendantsWithReferences();
		this._canTrash = total === 0;
	}

	override render() {
		const headline = this.localize.string('#actions_trash');

		return html`
			<uui-dialog-layout class="uui-text" headline=${headline}>
				${this._canTrash !== undefined
					? html`<p>${unsafeHTML(
							this._canTrash === false
								? this.localize.string('#defaultdialogs_cannotTrashWhenReferenced', this._name)
								: this.localize.string('#defaultdialogs_confirmTrash', this._name),
						)}</p>`
					: nothing}

				${this._referencesConfig
					? html`<umb-confirm-action-modal-entity-references
							.config=${this._referencesConfig}
							@change=${this.#onReferencesChange}></umb-confirm-action-modal-entity-references>`
					: nothing}

				<uui-button slot="actions" id="cancel" label="Cancel" @click=${this._rejectModal}></uui-button>

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

export { UmbTrashWithRelationConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-trash-with-relation-confirm-modal': UmbTrashWithRelationConfirmModalElement;
	}
}
