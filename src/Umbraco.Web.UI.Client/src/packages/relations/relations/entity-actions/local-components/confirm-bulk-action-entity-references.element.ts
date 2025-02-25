import type { UmbEntityReferenceRepository } from '../../reference/types.js';
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

@customElement('umb-confirm-bulk-action-modal-entity-references')
export class UmbConfirmBulkActionModalEntityReferencesElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	config?: {
		uniques: Array<string>;
		itemRepositoryAlias: string;
		referenceRepositoryAlias: string;
	};

	@state()
	_items: Array<any> = [];

	@state()
	_totalItems: number = 0;

	#itemRepository?: UmbItemRepository<any>;
	#referenceRepository?: UmbEntityReferenceRepository;

	#limitItems = 5;

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

		this.#loadAreReferenced();
	}

	async #loadAreReferenced() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		if (!this.#itemRepository) {
			throw new Error('Failed to create item repository.');
		}

		if (!this.config?.uniques) {
			throw new Error('Missing uniques in config.');
		}

		const { data } = await this.#referenceRepository.requestAreReferenced(this.config.uniques, 0, this.#limitItems);

		if (data) {
			this._totalItems = data.total;
			const uniques = data.items.map((item) => item.unique).filter((unique) => unique) as Array<string>;
			const { data: items } = await this.#itemRepository.requestItems(uniques);
			this._items = items ?? [];
		}
	}

	override render() {
		if (this._totalItems === 0) return nothing;

		return html`
			<h5 id="reference-headline">The following items are used by other content.</h5>
			<uui-ref-list>
				${this._items.map(
					(item) =>
						html`<umb-entity-item-ref
							.item=${item}
							readonly
							?standalone=${this._totalItems === 1}></umb-entity-item-ref> `,
				)}
			</uui-ref-list>
			${this._totalItems > this.#limitItems
				? html`<span
						>${this.localize.term('references_labelMoreReferences', this._totalItems - this.#limitItems)}</span
					>`
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

export { UmbConfirmBulkActionModalEntityReferencesElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirm-bulk-action-modal-entity-references': UmbConfirmBulkActionModalEntityReferencesElement;
	}
}
