import type { UmbEntityReferenceRepository, UmbReferenceItemModel } from '../reference/types.js';
import {
	html,
	customElement,
	css,
	state,
	nothing,
	type PropertyValues,
	property,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-confirm-action-modal-entity-references')
export class UmbConfirmActionModalEntityReferencesElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	config?: {
		itemRepositoryAlias: string;
		referenceRepositoryAlias: string;
		entityType: string;
		unique: string;
	};

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
		if (!this.config) {
			this.#itemRepository?.destroy();
			this.#referenceRepository?.destroy();
			return;
		}

		if (!this.config?.referenceRepositoryAlias) {
			throw new Error('Missing referenceRepositoryAlias in config.');
		}

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.config?.referenceRepositoryAlias,
		);

		if (!this.config?.itemRepositoryAlias) {
			throw new Error('Missing itemRepositoryAlias in config.');
		}

		this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.config.itemRepositoryAlias,
		);

		this.#loadReferencedBy();
		this.#loadDescendantsWithReferences();
	}

	async #loadReferencedBy() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		if (!this.config?.unique) {
			throw new Error('Missing unique in data.');
		}

		const { data } = await this.#referenceRepository.requestReferencedBy(this.config.unique, 0, this.#limitItems);

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

		if (!this.config?.unique) {
			throw new Error('Missing unique in data.');
		}

		const { data } = await this.#referenceRepository.requestDescendantsWithReferences(
			this.config.unique,
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
			${this.#renderItems('references_labelDependsOnThis', this._referencedByItems, this._totalReferencedByItems)}
			${this.#renderItems(
				'references_labelDependentDescendants',
				this._descendantsWithReferences,
				this._totalDescendantsWithReferences,
			)}
		`;
	}

	#renderItems(headline: string, items: Array<UmbReferenceItemModel>, total: number) {
		if (total === 0) return nothing;

		return html`
			<h5 class="uui-h5" id="reference-headline">${this.localize.term(headline)}</h5>
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
			#reference-headline {
				margin-bottom: var(--uui-size-3);
			}

			uui-ref-list {
				margin-bottom: var(--uui-size-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-action-modal-entity-references': UmbConfirmActionModalEntityReferencesElement;
	}
}
