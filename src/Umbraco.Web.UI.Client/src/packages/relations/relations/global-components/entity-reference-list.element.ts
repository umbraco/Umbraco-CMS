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

/**
 * Which kind of reference `umb-entity-reference-list` lists: the items referencing the entity (`referencedBy`), or
 * its descendants that are referenced elsewhere (`descendantsWithReferences`).
 */
export type UmbEntityReferenceListSource = 'referencedBy' | 'descendantsWithReferences';

interface UmbEntityReferenceListPage {
	total: number;
	items: Array<UmbReferenceItemModel | UmbEntityModel>;
}

/**
 * Presentational, paged list of the items referencing (or, in `descendantsWithReferences` mode, the descendants
 * referenced by) a given entity. Used by the entity references workspace info app and the entity references modal.
 * @element umb-entity-reference-list
 */
@customElement('umb-entity-reference-list')
export class UmbEntityReferenceListElement extends UmbLitElement {
	/**
	 * The unique identifier of the entity to look up references for. Setting it (re)loads the current `source`.
	 * @param {string | undefined} value - The entity unique.
	 */
	@property({ type: String, attribute: false })
	public set unique(value: string | undefined) {
		const oldValue = this.#unique;
		this.#unique = value;
		if (value === oldValue) return;
		this._currentPage = 1;

		if (!value) {
			this.#requestToken++;
			this._items = [];
			this._total = 0;
			return;
		}

		this.#getReferences();
	}
	public get unique(): string | undefined {
		return this.#unique;
	}
	#unique?: string;

	/**
	 * Alias of the {@link UmbEntityReferenceRepository} used to look up references.
	 */
	@property({ attribute: 'reference-repository-alias' })
	referenceRepositoryAlias?: string;

	/**
	 * Alias of the item repository used to resolve the descendant uniques returned by `requestDescendantsWithReferences`
	 * into presentable items. Only used when `source` is `descendantsWithReferences`.
	 */
	@property({ attribute: 'item-repository-alias' })
	itemRepositoryAlias?: string;

	/**
	 * Which kind of reference to list: the items referencing the entity, or its descendants that are referenced elsewhere.
	 */
	@property()
	source: UmbEntityReferenceListSource = 'referencedBy';

	/**
	 * How many items to show per page.
	 */
	@property({ type: Number, attribute: 'items-per-page' })
	itemsPerPage = 10;

	/**
	 * Whether the listed items are non-interactive (not clickable).
	 */
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
	#requestToken = 0;

	/**
	 * The total number of items for the current `source`, once loaded.
	 * @returns {number}
	 */
	getTotal(): number {
		return this._total;
	}

	protected override updated(changedProperties: PropertyValues): void {
		super.updated(changedProperties);
		const repositoryConfigChanged = ['referenceRepositoryAlias', 'itemRepositoryAlias', 'source'].some((key) =>
			changedProperties.has(key),
		);
		if (repositoryConfigChanged) {
			this.#configureRepositories();
		}
	}

	async #configureRepositories() {
		if (!this.referenceRepositoryAlias) return;

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.referenceRepositoryAlias,
		);

		this.#itemRepository =
			this.source === 'descendantsWithReferences' && this.itemRepositoryAlias
				? await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.itemRepositoryAlias)
				: undefined;

		this.#getReferences();
	}

	async #getReferences() {
		const unique = this.#unique;
		const repository = this.#referenceRepository;
		if (!unique || !repository) return;

		const token = ++this.#requestToken;
		const skip = (this._currentPage - 1) * this.itemsPerPage;

		const page =
			this.source === 'descendantsWithReferences'
				? await this.#fetchDescendantsWithReferences(repository, unique, skip)
				: await this.#fetchReferencedBy(repository, unique, skip);

		// A newer request (e.g. the unique or page changed again) has since started — its result should win, not ours.
		if (token !== this.#requestToken) return;

		if (page) {
			this._total = page.total;
			this._items = page.items;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #fetchReferencedBy(
		repository: UmbEntityReferenceRepository,
		unique: string,
		skip: number,
	): Promise<UmbEntityReferenceListPage | undefined> {
		const { data } = await repository.requestReferencedBy(unique, skip, this.itemsPerPage);
		return data;
	}

	async #fetchDescendantsWithReferences(
		repository: UmbEntityReferenceRepository,
		unique: string,
		skip: number,
	): Promise<UmbEntityReferenceListPage | undefined> {
		// If the repository does not have the method, there are no descendants to report.
		if (!repository.requestDescendantsWithReferences) return { total: 0, items: [] };

		const { data } = await repository.requestDescendantsWithReferences(unique, skip, this.itemsPerPage);
		if (!data) return undefined;

		return { total: data.total, items: await this.#resolveItems(data.items) };
	}

	async #resolveItems(items: Array<UmbEntityModel>): Promise<Array<UmbEntityModel>> {
		if (!this.#itemRepository) return items;

		const uniques = items.map((item) => item.unique).filter(Boolean) as Array<string>;
		const { data } = await this.#itemRepository.requestItems(uniques);
		return data ?? [];
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;
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
