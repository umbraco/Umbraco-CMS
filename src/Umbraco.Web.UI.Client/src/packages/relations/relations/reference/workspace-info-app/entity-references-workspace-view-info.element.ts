import type { UmbEntityReferenceRepository, UmbReferenceItemModel } from '../types.js';
import type { ManifestWorkspaceInfoAppEntityReferencesKind } from './types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-references-workspace-info-app')
export class UmbEntityReferencesWorkspaceInfoAppElement extends UmbLitElement {
	@property({ type: Object })
	private _manifest?: ManifestWorkspaceInfoAppEntityReferencesKind | undefined;
	public get manifest(): ManifestWorkspaceInfoAppEntityReferencesKind | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestWorkspaceInfoAppEntityReferencesKind | undefined) {
		this._manifest = value;
		this.#init();
	}

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceItemModel> = [];

	#itemsPerPage = 10;
	#referenceRepository?: UmbEntityReferenceRepository;
	#unique?: UmbEntityUnique;
	#workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUnique();
		});
	}

	async #init() {
		if (!this._manifest) return;
		const referenceRepositoryAlias = this._manifest.meta.referenceRepositoryAlias;

		if (!referenceRepositoryAlias) {
			throw new Error('Reference repository alias is required');
		}

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			referenceRepositoryAlias,
		);

		this.#getReferences();
	}

	#observeUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				if (!unique) {
					this.#unique = undefined;
					this._items = [];
					return;
				}

				if (this.#unique === unique) {
					return;
				}

				this.#unique = unique;
				this.#getReferences();
			},
			'umbEntityReferencesUniqueObserver',
		);
	}

	async #getReferences() {
		if (!this.#unique) return;
		if (!this.#referenceRepository) return;

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.#unique,
			(this._currentPage - 1) * this.#itemsPerPage,
			this.#itemsPerPage,
		);
		if (!data) return;

		this._total = data.total;
		this._items = data.items;
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
				${this.#renderItems()} ${this.#renderReferencePagination()}
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

	#renderReferencePagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);
		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination-container">
				<uui-pagination .total=${totalPages} @change="${this.#onPageChange}"></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			.pagination-container {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-4);
			}

			uui-pagination {
				flex: 1;
				display: inline-block;
			}
		`,
	];
}

export { UmbEntityReferencesWorkspaceInfoAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-references-workspace-info-app': UmbEntityReferencesWorkspaceInfoAppElement;
	}
}
