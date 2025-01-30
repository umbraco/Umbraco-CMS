import { UmbDocumentReferenceRepository } from '../repository/index.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../constants.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { isDefaultReference, isDocumentReference, isMediaReference } from '@umbraco-cms/backoffice/relations';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbReferenceModel } from '@umbraco-cms/backoffice/relations';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-document-references-workspace-info-app')
export class UmbDocumentReferencesWorkspaceInfoAppElement extends UmbLitElement {
	@state()
	private _editDocumentPath = '';

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceModel> = [];

	#itemsPerPage = 10;
	#referenceRepository = new UmbDocumentReferenceRepository(this);
	#documentUnique?: UmbEntityUnique;
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentPath = routeBuilder({});
			});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeDocumentUnique();
		});
	}

	#observeDocumentUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				if (!unique) {
					this.#documentUnique = undefined;
					this._items = [];
					return;
				}

				if (this.#documentUnique === unique) {
					return;
				}

				this.#documentUnique = unique;
				this.#getReferences();
			},
			'umbReferencesDocumentUniqueObserver',
		);
	}

	async #getReferences() {
		if (!this.#documentUnique) {
			throw new Error('Document unique is required');
		}

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.#documentUnique,
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
		if (!this._items?.length) return nothing;
		return html`
			<umb-workspace-info-app-layout headline="#references_labelUsedByItems">
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
						(item) => html`
							<uui-table-row>
								<uui-table-cell style="text-align:center;">
									<umb-icon name=${this.#getIcon(item)}></umb-icon>
								</uui-table-cell>
								<uui-table-cell class="link-cell">
									${when(
										isDocumentReference(item),
										() => html`
											<uui-button
												label="${this.localize.term('general_edit')} ${item.name}"
												href="${this._editDocumentPath}edit/${item.id}">
												${item.name}
											</uui-button>
										`,
										() => item.name,
									)}
								</uui-table-cell>
								<uui-table-cell>
									${this.#getPublishedStatus(item)
										? this.localize.term('content_published')
										: this.localize.term('content_unpublished')}
								</uui-table-cell>
								<uui-table-cell>${this.#getContentTypeName(item)}</uui-table-cell>
								<uui-table-cell>${this.#getContentType(item)}</uui-table-cell>
							</uui-table-row>
						`,
					)}
				</uui-table>
				${this.#renderReferencePagination()}
			</umb-workspace-info-app-layout>
		`;
	}

	#renderReferencePagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);
		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination">
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

export default UmbDocumentReferencesWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-references-workspace-info-app': UmbDocumentReferencesWorkspaceInfoAppElement;
	}
}
