import type { UmbEntityReferenceRepository, UmbReferenceItemModel } from '../reference/types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

export type UmbEntityReferenceListSource = 'referencedBy' | 'descendantsWithReferences' | 'needingAttention';

// Properties that require re-running #init() when they change — see updated() below.
const REPOSITORY_PROPERTIES = ['referenceRepositoryAlias', 'itemRepositoryAlias', 'source'] as const;

/**
 * Presentational, paged list of the items referencing (or, in `descendantsWithReferences` mode, the descendants
 * referenced by) a given entity. Used by the entity references workspace info app and the entity references modal.
 * @element umb-entity-reference-list
 */
@customElement('umb-entity-reference-list')
export class UmbEntityReferenceListElement extends UmbLitElement {
	@property({ type: String, attribute: false })
	public set unique(value: string | undefined) {
		const oldValue = this.#unique;
		this.#unique = value;
		if (value === oldValue) return;
		this._currentPage = 1;
		// Clear before fetching, not just when there's no new value — otherwise the previous entity's
		// items/total stay on screen (and visible to @change consumers) until the new fetch resolves.
		this._items = [];
		this._total = 0;

		if (!value) return;

		this.#getReferences();
	}
	public get unique(): string | undefined {
		return this.#unique;
	}
	#unique?: string;

	/**
	 * A pre-resolved, statically-paged set of items to render — e.g. entities resolved client-side from a
	 * workspace draft. Setting this switches the element out of `unique`/`source`-driven fetch mode.
	 */
	@property({ type: Array, attribute: false })
	public set items(value: Array<UmbReferenceItemModel | UmbEntityModel> | undefined) {
		this.#staticItems = value;
		this._currentPage = 1;
		this._total = value?.length ?? 0;
		this.#renderStaticItemsPage();
		this.dispatchEvent(new UmbChangeEvent());
	}
	public get items(): Array<UmbReferenceItemModel | UmbEntityModel> | undefined {
		return this.#staticItems;
	}
	#staticItems?: Array<UmbReferenceItemModel | UmbEntityModel>;

	@property({ attribute: 'reference-repository-alias' })
	referenceRepositoryAlias?: string;

	@property({ attribute: 'item-repository-alias' })
	itemRepositoryAlias?: string;

	@property()
	source: UmbEntityReferenceListSource = 'referencedBy';

	@property({ type: Number, attribute: 'items-per-page' })
	itemsPerPage = 10;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceItemModel | UmbEntityModel>;

	#referenceRepository?: UmbEntityReferenceRepository;
	#itemRepository?: UmbItemRepository<any>;

	/**
	 * The total number of items for the current `source`, once loaded.
	 * @returns {number}
	 */
	getTotal(): number {
		return this._total;
	}

	protected override updated(changedProperties: PropertyValues): void {
		super.updated(changedProperties);

		// Runs on the first update too — every reactive property counts as "changed" then. Re-running #init()
		// when these change later covers consumers (e.g. the workspace info app) whose extension-provided
		// referenceRepositoryAlias can arrive or change after this element's first render.
		if (REPOSITORY_PROPERTIES.some((prop) => changedProperties.has(prop))) {
			this.#init();
		}
	}

	async #init() {
		// Not an error: with re-init reacting to property changes (see `updated()`), this runs on the very first
		// update too, where an extension-provided alias (e.g. from a workspace info app's manifest) may not have
		// arrived yet. It re-runs once the alias is actually set.
		if (!this.referenceRepositoryAlias) return;

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.referenceRepositoryAlias,
		);

		if (this.source === 'descendantsWithReferences' && this.itemRepositoryAlias) {
			this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.itemRepositoryAlias);
		}

		this.#getReferences();
	}

	async #getReferences() {
		if (!this.#unique) return;
		if (!this.#referenceRepository) return;

		const skip = (this._currentPage - 1) * this.itemsPerPage;

		if (this.source === 'descendantsWithReferences') {
			await this.#getDescendantsWithReferences(skip);
		} else {
			await this.#getReferencedBy(skip);
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #getReferencedBy(skip: number) {
		if (!this.#referenceRepository || !this.#unique) return;

		const { data } = await this.#referenceRepository.requestReferencedBy(this.#unique, skip, this.itemsPerPage);
		if (!data) return;

		this._total = data.total;
		this._items = data.items;
	}

	async #getDescendantsWithReferences(skip: number) {
		if (!this.#referenceRepository || !this.#unique) return;

		// If the repository does not have the method, there are no descendants to report.
		if (!this.#referenceRepository.requestDescendantsWithReferences) {
			this._total = 0;
			this._items = [];
			return;
		}

		const { data } = await this.#referenceRepository.requestDescendantsWithReferences(
			this.#unique,
			skip,
			this.itemsPerPage,
		);
		if (!data) return;

		this._total = data.total;

		if (!this.#itemRepository) {
			this._items = data.items;
			return;
		}

		const uniques = data.items.map((item) => item.unique).filter(Boolean) as Array<string>;
		const { data: items } = await this.#itemRepository.requestItems(uniques);
		this._items = items ?? [];
	}

	#renderStaticItemsPage() {
		if (!this.#staticItems) return;
		const skip = (this._currentPage - 1) * this.itemsPerPage;
		this._items = this.#staticItems.slice(skip, skip + this.itemsPerPage);
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;

		if (this.#staticItems) {
			this.#renderStaticItemsPage();
			return;
		}

		this.#getReferences();
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderPagination()}`;
	}

	#renderItems() {
		if (!this._items) return nothing;
		if (this._items.length === 0) {
			return html`<umb-localize class="no-items" key="references_itemHasNoReferences"></umb-localize>`;
		}
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => html`<umb-entity-item-ref .item=${item} ?readonly=${this.readonly}></umb-entity-item-ref>`,
				)}
			</uui-ref-list>
		`;
	}

	#renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.itemsPerPage);
		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination-container">
				<uui-pagination
					.total=${totalPages}
					firstlabel=${this.localize.term('general_first')}
					previouslabel=${this.localize.term('general_previous')}
					nextlabel=${this.localize.term('general_next')}
					lastlabel=${this.localize.term('general_last')}
					@change=${this.#onPageChange}></uui-pagination>
			</div>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			.no-items {
				display: block;
			}

			.pagination-container {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

export default UmbEntityReferenceListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-reference-list': UmbEntityReferenceListElement;
	}
}
