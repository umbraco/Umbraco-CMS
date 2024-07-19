import type { UmbCreatedPackage } from '../../../types.js';
import { html, css, nothing, customElement, state, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPackageRepository } from '@umbraco-cms/backoffice/package';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-packages-created-overview')
export class UmbPackagesCreatedOverviewElement extends UmbLitElement {
	#take = 10;

	@state()
	private _loading = true;

	@state()
	private _createdPackages: Array<UmbCreatedPackage> = [];

	@state()
	private _currentPage = 1;

	@state()
	private _total?: number;

	#packageRepository = new UmbPackageRepository(this);

	constructor() {
		super();
		this.#getPackages();
	}

	async #getPackages() {
		const skip = this._currentPage * this.#take - this.#take;
		const data = await this.#packageRepository.getCreatedPackages({ skip, take: this.#take });

		if (data) {
			this._createdPackages = data.items;
			this._total = data.total;
		}

		this._loading = false;
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;
		this.#getPackages();
	}

	async #deletePackage(pkg: UmbCreatedPackage) {
		if (!pkg.unique) return;

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `Remove ${pkg.name}?`,
			content: 'Are you sure you want to delete this package',
			confirmLabel: this.localize.term('general_delete'),
		});

		const success = await this.#packageRepository.deleteCreatedPackage(pkg.unique);
		if (!success) return;

		const index = this._createdPackages.findIndex((x) => x.unique === pkg.unique);
		this._createdPackages.splice(index, 1);
		this.requestUpdate();
	}

	override render() {
		return html`
			<uui-button
				look="primary"
				href="section/packages/view/created/package-builder"
				label=${this.localize.term('packager_createPackage')}></uui-button>
			${when(
				this._loading,
				() => html`<div class="container"><uui-loader></uui-loader></div>`,
				() => this.#renderCreatedPackages(),
			)}
			${this.#renderPagination()}
		`;
	}

	#renderCreatedPackages() {
		if (!this._createdPackages.length) return this.#renderNoPackages();
		return html`
			<uui-box headline="Created packages" style="--uui-box-default-padding:0;">
				<uui-ref-list>
					${repeat(
						this._createdPackages,
						(item) => item.unique,
						(item) => this.#renderItem(item),
					)}
				</uui-ref-list>
			</uui-box>
		`;
	}

	#renderNoPackages() {
		return html`<h2 class="no-packages">${this.localize.term('packager_noPackagesCreated')}</h2>`;
	}

	#renderItem(pkg: UmbCreatedPackage) {
		return html`
			<uui-ref-node-package name=${pkg.name} @open=${() => this.#packageBuilder(pkg)}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#deletePackage(pkg)}
						label=${this.localize.term('general_delete')}></uui-button>
				</uui-action-bar>
			</uui-ref-node-package>
		`;
	}

	#packageBuilder(pkg: UmbCreatedPackage) {
		if (!pkg.unique) return;
		window.history.pushState({}, '', `section/packages/view/created/package-builder/${pkg.unique}`);
	}

	#renderPagination() {
		if (!this._total) return nothing;
		const totalPages = Math.ceil(this._total / this.#take);
		if (totalPages <= 1) return nothing;
		return html`
			<div class="container">
				<uui-pagination .total=${totalPages} @change=${this.#onPageChange}></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
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

			.container {
				display: flex;
				justify-content: center;
			}
		`,
	];
}

export default UmbPackagesCreatedOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-created-overview': UmbPackagesCreatedOverviewElement;
	}
}
