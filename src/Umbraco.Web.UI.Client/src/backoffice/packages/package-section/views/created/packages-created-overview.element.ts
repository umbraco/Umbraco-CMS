import { html, css, nothing } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UUIPaginationEvent } from '@umbraco-ui/uui';
import { PackageDefinitionResponseModel, PackageResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';

@customElement('umb-packages-created-overview')
export class UmbPackagesCreatedOverviewElement extends UmbLitElement {
	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
			uui-box {
				margin: var(--uui-size-space-5) 0;
				padding-bottom: var(--uui-size-space-1);
			}

			.no-packages {
				display: flex;
				justify-content: space-around;
			}
			uui-pagination {
				display: inline-block;
			}

			.pagination,
			.loading {
				display: flex;
				justify-content: center;
			}
		`,
	];

	private take = 20;

	@state()
	private _loading = true;

	@state()
	private _createdPackages: PackageDefinitionResponseModel[] = [];

	@state()
	private _currentPage = 1;

	@state()
	private _total?: number;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#getPackages();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	async #getPackages() {
		const skip = this._currentPage * this.take - this.take;
		const { data } = await tryExecuteAndNotify(this, PackageResource.getPackageCreated({ skip, take: this.take }));
		if (data) {
			this._total = data.total;
			this._createdPackages = data.items;
		}
		this._loading = false;
	}

	#createNewPackage() {
		window.history.pushState({}, '', `/section/packages/view/created/package-builder`);
	}

	render() {
		return html`<uui-button look="primary" @click="${this.#createNewPackage}" label="Create package">
				Create package
			</uui-button>
			${this._loading ? html`<uui-loader class="loading"></uui-loader>` : this.#renderCreatedPackages()}
			${this.#renderPagination()}`;
	}

	#renderCreatedPackages() {
		if (!this._createdPackages.length) return html`<h2 class="no-packages">No packages have been created yet</h2>`;

		return html`<uui-box headline="Created packages" style="--uui-box-default-padding:0;">
			<uui-ref-list>
				${repeat(
					this._createdPackages,
					(item) => item.key,
					(item) => this.#renderPackageItem(item)
				)}
			</uui-ref-list>
		</uui-box>`;
	}

	#renderPackageItem(p: PackageDefinitionResponseModel) {
		return html`<uui-ref-node-package name=${ifDefined(p.name)} @open="${() => this.#packageBuilder(p)}">
			<uui-action-bar slot="actions">
				<uui-button @click=${() => this.#deletePackage(p)} label="Delete package">
					<uui-icon name="delete"></uui-icon>
				</uui-button>
			</uui-action-bar>
		</uui-ref-node-package>`;
	}

	#packageBuilder(p: PackageDefinitionResponseModel) {
		if (!p.key) return;
		window.history.pushState({}, '', `/section/packages/view/created/package-builder/${p.key}`);
	}

	#renderPagination() {
		if (!this._total) return nothing;
		const totalPages = Math.ceil(this._total / this.take);
		if (totalPages <= 1) return nothing;
		return html`<div class="pagination">
			<uui-pagination .total="${totalPages}" @change="${this.#onPageChange}"></uui-pagination>
		</div>`;
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;
		this.#getPackages();
	}

	async #deletePackage(p: PackageDefinitionResponseModel) {
		if (!p.key) return;
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${p.name}?`,
			content: 'Are you sure you want to delete this package',
			confirmLabel: 'Delete',
		});

		await modalHandler?.onSubmit();

		const { error } = await tryExecuteAndNotify(this, PackageResource.deletePackageCreatedByKey({ key: p.key }));
		if (error) return;
		const index = this._createdPackages.findIndex((x) => x.key === p.key);
		this._createdPackages.splice(index, 1);
		this.requestUpdate();
	}
}

export default UmbPackagesCreatedOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-created-overview': UmbPackagesCreatedOverviewElement;
	}
}
