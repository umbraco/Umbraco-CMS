import { css, html, customElement, state, nothing, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { RelationItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDocumentTrackedReferenceRepository } from '@umbraco-cms/backoffice/document';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-workspace-view-info-reference')
export class UmbDocumentWorkspaceViewInfoReferenceElement extends UmbLitElement {
	#itemsPerPage = 10;
	#trackedReferenceRepository;

	@property()
	documentUnique = '';

	@state()
	private _editDocumentPath = '';

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<RelationItemResponseModel> = [];

	constructor() {
		super();
		this.#trackedReferenceRepository = new UmbDocumentTrackedReferenceRepository(this);

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	protected firstUpdated(): void {
		this.#getReferences();
	}

	async #getReferences() {
		const { data } = await this.#trackedReferenceRepository.requestTrackedReference(
			this.documentUnique,
			this._currentPage - 1 * this.#itemsPerPage,
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

	render() {
		if (this._items && this._items.length > 0) {
			return html`<strong>
					<umb-localize key="references_labelUsedByItems">Referenced by the following items</umb-localize>
				</strong>
				<uui-box style="--uui-box-default-padding:0">
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_name">Name</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_status">Status</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_typeName">Type Name</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_type">Type</umb-localize></uui-table-head-cell>
							<uui-table-head-cell>
								<umb-localize key="relationType_relation">Relation</umb-localize>
							</uui-table-head-cell>
						</uui-table-head>

						${repeat(
							this._items,
							(item) => item.nodeId,
							(item) =>
								html`<uui-table-row>
									<uui-table-cell>
										<uui-icon style=" vertical-align: middle;" name="icon-document"></uui-icon>
									</uui-table-cell>
									<uui-table-cell class="link-cell">
										<uui-button label="Edit" href=${`${this._editDocumentPath}edit/${item.nodeId}`}>
											${item.nodeName}
										</uui-button>
									</uui-table-cell>
									<uui-table-cell>
										${item.nodePublished
											? this.localize.term('content_published')
											: this.localize.term('content_unpublished')}
									</uui-table-cell>
									<uui-table-cell>${item.contentTypeName}</uui-table-cell>
									<uui-table-cell>${item.nodeType}</uui-table-cell>
									<uui-table-cell>${item.relationTypeName}</uui-table-cell>
								</uui-table-row>`,
						)}
					</uui-table>
				</uui-box>
				${this.#renderReferencePagination()}`;
		} else {
			return nothing;
		}
	}

	#renderReferencePagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`<div class="pagination">
			<uui-pagination .total=${totalPages} @change="${this.#onPageChange}"></uui-pagination>
		</div>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.link-cell {
				font-weight: bold;
			}

			uui-table-cell:not(.link-cell) {
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

export default UmbDocumentWorkspaceViewInfoReferenceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-info-reference': UmbDocumentWorkspaceViewInfoReferenceElement;
	}
}
