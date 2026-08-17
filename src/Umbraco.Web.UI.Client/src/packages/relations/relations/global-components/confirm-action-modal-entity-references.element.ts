import { UMB_ENTITY_REFERENCES_MODAL } from '../reference/modal/constants.js';
import type { UmbEntityReferenceRepository, UmbReferenceItemModel } from '../reference/types.js';
import { customElement, css, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export interface UmbConfirmActionModalEntityReferencesConfig {
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
	unique: string;
}

@customElement('umb-confirm-action-modal-entity-references')
export class UmbConfirmActionModalEntityReferencesElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	config?: UmbConfirmActionModalEntityReferencesConfig;

	@state()
	private _referencedByItems: Array<UmbReferenceItemModel> = [];

	@state()
	private _totalReferencedByItems: number = 0;

	@state()
	private _totalDescendantsWithReferences: number = 0;

	@state()
	private _descendantsWithReferences: Array<any> = [];

	#itemRepository?: UmbItemRepository<any>;
	#referenceRepository?: UmbEntityReferenceRepository;

	#limitItems = 3;

	getTotalReferencedBy() {
		return this._totalReferencedByItems;
	}

	getTotalDescendantsWithReferences() {
		return this._totalDescendantsWithReferences;
	}

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

		await Promise.all([this.#loadReferencedBy(), this.#loadDescendantsWithReferences()]);
		this.dispatchEvent(new UmbChangeEvent());
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

	#onClickViewAll(event: Event) {
		event.preventDefault();
		if (!this.config) return;

		umbOpenModal(this, UMB_ENTITY_REFERENCES_MODAL, {
			data: {
				unique: this.config.unique,
				referenceRepositoryAlias: this.config.referenceRepositoryAlias,
				itemRepositoryAlias: this.config.itemRepositoryAlias,
			},
		}).catch(() => undefined);
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
			<h5 class="uui-h5">${this.localize.term(headline)}</h5>
			<uui-ref-list>
				${repeat(
					items,
					(item) => item.unique,
					(item) => html`<umb-entity-item-ref .item=${item} readonly></umb-entity-item-ref>`,
				)}
			</uui-ref-list>
			${when(
				total > this.#limitItems,
				() => html`
					<uui-button
						look="default"
						label=${this.localize.term('references_labelMoreReferences', total - this.#limitItems)}
						@click=${this.#onClickViewAll}></uui-button>
				`,
			)}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-ref-list {
				margin-top: var(--uui-size-3);
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
