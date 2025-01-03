import { UmbMediaReferenceRepository } from '../../../reference/index.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { isDefaultReference, isDocumentReference, isMediaReference } from '@umbraco-cms/backoffice/relations';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbReferenceModel } from '@umbraco-cms/backoffice/relations';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-media-workspace-view-info-reference')
export class UmbMediaWorkspaceViewInfoReferenceElement extends UmbLitElement {
	#itemsPerPage = 10;

	#referenceRepository;

	#routeBuilder?: UmbModalRouteBuilder;

	@property()
	mediaUnique = '';

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceModel> = [];

	@state()
	private _loading = true;

	constructor() {
		super();
		this.#referenceRepository = new UmbMediaReferenceRepository(this);

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#routeBuilder = routeBuilder;
			});
	}

	protected override firstUpdated(): void {
		this.#getReferences();
	}

	async #getReferences() {
		this._loading = true;

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.mediaUnique,
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

	#getEditPath(item: UmbReferenceModel) {
		const entityType = this.#getEntityType(item);
		return this.#routeBuilder && entityType ? `${this.#routeBuilder({ entityType })}edit/${item.id}` : '#';
	}

	#getIcon(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return item.documentType.icon ?? 'icon-document';
		}
		if (isMediaReference(item)) {
			return item.mediaType.icon ?? 'icon-picture';
		}
		if (isDefaultReference(item)) {
			return item.icon ?? 'icon-document';
		}
		return 'icon-document';
	}

	#getPublishedStatus(item: UmbReferenceModel) {
		return isDocumentReference(item) ? item.published : true;
	}

	#getContentTypeName(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return item.documentType.name;
		}
		if (isMediaReference(item)) {
			return item.mediaType.name;
		}
		if (isDefaultReference(item)) {
			return item.type;
		}
		return null;
	}

	#getEntityType(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return 'document';
		}
		if (isMediaReference(item)) {
			return 'media';
		}
		if (isDefaultReference(item)) {
			return item.type;
		}
		return null;
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('references_labelUsedByItems')}>
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => html`${this.#renderItems()} ${this.#renderPagination()}`,
				)}
			</uui-box>
		`;
	}

	#renderItems() {
		if (!this._items?.length)
			return html`<p>
				<umb-localize key="references_itemHasNoReferences">This item has no references.</umb-localize>
			</p>`;
		return html`
			<uui-table>
				<uui-table-head>
					<uui-table-head-cell><umb-localize key="general_name">Name</umb-localize></uui-table-head-cell>
					<uui-table-head-cell><umb-localize key="general_status">Status</umb-localize></uui-table-head-cell>
					<uui-table-head-cell><umb-localize key="general_typeName">Type Name</umb-localize></uui-table-head-cell>
					<uui-table-head-cell><umb-localize key="general_type">Type</umb-localize></uui-table-head-cell>
				</uui-table-head>
				${repeat(
					this._items,
					(item) => item.id,
					(item) => html`
						<uui-table-row>
							<uui-table-cell>
								<uui-ref-node name=${item.name!} href=${this.#getEditPath(item)}>
									<umb-icon slot="icon" name=${this.#getIcon(item)}></umb-icon>
								</uui-ref-node>
							</uui-table-cell>
							<uui-table-cell>
								${when(
									this.#getPublishedStatus(item),
									() =>
										html`<uui-tag color="positive" look="secondary"
											>${this.localize.term('content_published')}</uui-tag
										>`,
									() =>
										html`<uui-tag color="default" look="secondary"
											>${this.localize.term('content_unpublished')}</uui-tag
										>`,
								)}
							</uui-table-cell>
							<uui-table-cell>${this.#getContentTypeName(item)}</uui-table-cell>
							<uui-table-cell>${this.#getEntityType(item)}</uui-table-cell>
						</uui-table-row>
					`,
				)}
			</uui-table>
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

export default UmbMediaWorkspaceViewInfoReferenceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-info-reference': UmbMediaWorkspaceViewInfoReferenceElement;
	}
}
