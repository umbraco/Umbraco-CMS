import type { UmbEntityReferenceRepository, UmbReferenceItemModel } from '../../types.js';
import type {
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue,
} from './trash-with-relation-modal.token.js';
import { html, customElement, css, state, nothing, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

@customElement('umb-trash-with-relation-confirm-modal')
export class UmbTrashWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue
> {
	@state()
	_name?: string;

	@state()
	_referencedBy: Array<UmbReferenceItemModel> = [];

	@state()
	_totalReferencedBy: number = 0;

	#itemRepository?: UmbItemRepository<UmbNamedEntityModel>;
	#referenceRepository?: UmbEntityReferenceRepository;

	#limitReferencedBy = 3;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#initData();
	}

	async #initData() {
		if (!this.data) {
			this.#itemRepository?.destroy();
			this.#referenceRepository?.destroy();
			return;
		}

		this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<UmbNamedEntityModel>>(
			this,
			this.data.itemRepositoryAlias,
		);

		const { data } = await this.#itemRepository.requestItems([this.data.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		this._name = item.name;

		this.#loadReferencedBy();
	}

	async #loadReferencedBy() {
		if (!this.data?.referenceRepositoryAlias) {
			throw new Error('Missing referenceRepositoryAlias in data.');
		}

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.data?.referenceRepositoryAlias,
		);

		const { data: referencesData } = await this.#referenceRepository.requestReferencedBy(
			this.data.unique,
			0,
			this.#limitReferencedBy,
		);

		if (referencesData) {
			this._referencedBy = [...referencesData.items];
			this._totalReferencedBy = referencesData.total;
		}
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline="Trash">
				<p>Are you sure you want to move <strong>${this._name}</strong> to the recycle bin?</p>
				${this.#renderReferencedBy()}

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

	#renderReferencedBy() {
		if (this._totalReferencedBy === 0) return nothing;

		return html`
			<h5 id="reference-headline">${this.localize.term('references_labelDependsOnThis')}</h5>
			<uui-ref-list>
				${this._referencedBy.map(
					(reference) => html`<umb-entity-item-ref .item=${reference} readonly></umb-entity-item-ref> `,
				)}
			</uui-ref-list>
			${this._totalReferencedBy > this.#limitReferencedBy
				? html`<span
						>${this.localize.term(
							'references_labelMoreReferences',
							this._totalReferencedBy - this.#limitReferencedBy,
						)}</span
					>`
				: nothing}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}

			#reference-headline {
				margin-bottom: var(--uui-size-3);
			}

			uui-ref-list {
				margin-bottom: var(--uui-size-2);
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
