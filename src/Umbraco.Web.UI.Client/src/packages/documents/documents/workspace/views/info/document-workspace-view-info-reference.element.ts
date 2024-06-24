import { UmbDocumentReferenceRepository } from '../../../reference/index.js';
import { css, html, customElement, state, nothing, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import {
	isDefaultReference,
	isDocumentReference,
	isMediaReference,
	type UmbReferenceModel,
} from '@umbraco-cms/backoffice/relations';

@customElement('umb-document-workspace-view-info-reference')
export class UmbDocumentWorkspaceViewInfoReferenceElement extends UmbLitElement {
	#itemsPerPage = 10;
	#referenceRepository;

	@property()
	documentUnique = '';

	@state()
	private _editDocumentPath = '';

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceModel> = [];

	constructor() {
		super();
		this.#referenceRepository = new UmbDocumentReferenceRepository(this);

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});
	}

	protected override firstUpdated(): void {
		this.#getReferences();
	}

	async #getReferences() {
		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.documentUnique,
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
		return '';
	}

	#getContentType(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return item.documentType.alias;
		}
		if (isMediaReference(item)) {
			return item.mediaType.alias;
		}
		if (isDefaultReference(item)) {
			return item.type;
		}
		return '';
	}

	override render() {
		if (this._items && this._items.length > 0) {
			return html` <uui-box
					headline=${this.localize.term('references_labelUsedByItems')}
					style="--uui-box-default-padding:0">
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_name">Name</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_status">Status</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_typeName">Type Name</umb-localize></uui-table-head-cell>
							<uui-table-head-cell><umb-localize key="general_type">Type</umb-localize></uui-table-head-cell>
						</uui-table-head>

						${repeat(
							this._items,
							(item) => item.id,
							(item) =>
								html`<uui-table-row>
									<uui-table-cell style="text-align:center;">
										<umb-icon name=${this.#getIcon(item)}></umb-icon>
									</uui-table-cell>
									<uui-table-cell class="link-cell">
										${isDocumentReference(item)
											? html` <uui-button
													label="${this.localize.term('general_edit')} ${item.name}"
													href=${`${this._editDocumentPath}edit/${item.id}`}>
													${item.name}
												</uui-button>`
											: item.name}
									</uui-table-cell>
									<uui-table-cell>
										${this.#getPublishedStatus(item)
											? this.localize.term('content_published')
											: this.localize.term('content_unpublished')}
									</uui-table-cell>
									<uui-table-cell>${this.#getContentTypeName(item)}</uui-table-cell>
									<uui-table-cell>${this.#getContentType(item)}</uui-table-cell>
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

	static override styles = [
		UmbTextStyles,
		css`
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
