import { UmbMediaReferenceRepository } from '../repository/index.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-media-references-workspace-info-app')
export class UmbMediaReferencesWorkspaceInfoAppElement extends UmbLitElement {
	#itemsPerPage = 10;

	#referenceRepository;

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceItemModel> = [];

	@state()
	private _loading = true;

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;
	#mediaUnique?: UmbEntityUnique;

	constructor() {
		super();
		this.#referenceRepository = new UmbMediaReferenceRepository(this);

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeMediaUnique();
		});
	}

	#observeMediaUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				if (!unique) {
					this.#mediaUnique = undefined;
					this._items = [];
					return;
				}

				if (this.#mediaUnique === unique) {
					return;
				}

				this.#mediaUnique = unique;
				this.#getReferences();
			},
			'umbReferencesDocumentUniqueObserver',
		);
	}

	async #getReferences() {
		if (!this.#mediaUnique) {
			throw new Error('Media unique is required');
		}

		this._loading = true;

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.#mediaUnique,
			(this._currentPage - 1) * this.#itemsPerPage,
			this.#itemsPerPage,
		);

		if (!data) return;

		this._total = data.total;
		this._items = data.items;

		this._loading = false;
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;

		this.#getReferences();
	}

	override render() {
		if (!this._items?.length) return nothing;
		return html`
			<umb-workspace-info-app-layout headline="#references_labelUsedByItems">
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => html`${this.#renderItems()} ${this.#renderPagination()}`,
				)}
			</umb-workspace-info-app-layout>
		`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => html`<umb-entity-item-ref .item=${item}></umb-entity-item-ref>`,
				)}
			</uui-ref-list>
		`;
	}

	#renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination">
				<uui-pagination .total=${totalPages} @change=${this.#onPageChange}></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			uui-table-cell {
				color: var(--uui-color-text-alt);
			}

			uui-pagination {
				flex: 1;
				display: inline-block;
			}

			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

export default UmbMediaReferencesWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-references-workspace-info-app': UmbMediaReferencesWorkspaceInfoAppElement;
	}
}
