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

@customElement('umb-trash-with-relation-confirm-modal')
export class UmbTrashWithRelationConfirmModalElement extends UmbModalBaseElement<
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue
> {
	@state()
	_name?: string;

	@state()
	_referencedByItems: Array<UmbReferenceItemModel> = [];

	@state()
	_totalReferencedByItems: number = 0;

	@state()
	_totalDescendantsWithReferences: number = 0;

	@state()
	_descendantsWithReferences: Array<any> = [];

	#itemRepository?: UmbItemRepository<any>;
	#referenceRepository?: UmbEntityReferenceRepository;

	#limitItems = 3;

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

		this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);

		const { data } = await this.#itemRepository.requestItems([this.data.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		this._name = item.name;

		if (!this.data?.referenceRepositoryAlias) {
			throw new Error('Missing referenceRepositoryAlias in data.');
		}

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.data?.referenceRepositoryAlias,
		);

		this.#loadReferencedBy();
		this.#loadDescendantsWithReferences();
	}

	async #loadReferencedBy() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		if (!this.data?.unique) {
			throw new Error('Missing unique in data.');
		}

		const { data } = await this.#referenceRepository.requestReferencedBy(this.data.unique, 0, this.#limitItems);

		if (data) {
			this._referencedByItems = [...data.items];
			this._totalReferencedByItems = data.total;
		}
	}

	async #loadDescendantsWithReferences() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		if (!this.#itemRepository) {
			throw new Error('Failed to create item repository.');
		}

		// If the repository does not have the method, we don't need to load the referenced descendants.
		if (!this.#referenceRepository.requestDescendantsWithReferences) return;

		if (!this.data?.unique) {
			throw new Error('Missing unique in data.');
		}

		const { data } = await this.#referenceRepository.requestDescendantsWithReferences(
			this.data.unique,
			0,
			this.#limitItems,
		);

		if (data) {
			this._totalDescendantsWithReferences = data.total;
			const uniques = data.items.map((item) => item.unique).filter((unique) => unique) as Array<string>;
			const { data: items } = await this.#itemRepository.requestItems(uniques);
			this._descendantsWithReferences = items ?? [];
		}
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline="Trash">
				<p>Are you sure you want to move <strong>${this._name}</strong> to the recycle bin?</p>
				${this.#renderItems('references_labelDependsOnThis', this._referencedByItems, this._totalReferencedByItems)}
				${this.#renderItems(
					'references_labelDependentDescendants',
					this._descendantsWithReferences,
					this._totalDescendantsWithReferences,
				)}

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

	#renderItems(headline: string, items: Array<UmbReferenceItemModel>, total: number) {
		if (total === 0) return nothing;

		return html`
			<h5 id="reference-headline">${this.localize.term(headline)}</h5>
			<uui-ref-list>
				${items.map(
					(item) =>
						html`<umb-entity-item-ref .item=${item} readonly ?standalone=${total === 1}></umb-entity-item-ref> `,
				)}
			</uui-ref-list>
			${total > this.#limitItems
				? html`<span>${this.localize.term('references_labelMoreReferences', total - this.#limitItems)}</span>`
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
