import { UmbPackageRepository } from '../../../package/repository/package.repository.js';
import type { UmbPackageWithMigrationStatus } from '../../../types.js';
import { html, css, customElement, state, repeat, nothing, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';

import './installed-packages-section-view-item.element.js';

@customElement('umb-installed-packages-section-view')
export class UmbInstalledPackagesSectionViewElement extends UmbLitElement implements UmbSectionViewElement {
	@state()
	private _installedPackages: UmbPackageWithMigrationStatus[] = [];

	@state()
	private _migrationPackages: UmbPackageWithMigrationStatus[] = [];

	#packageRepository: UmbPackageRepository;

	constructor() {
		super();
		this.#packageRepository = new UmbPackageRepository(this);
	}

	override firstUpdated() {
		this.#loadInstalledPackages();
	}

	async #loadInstalledPackages() {
		const data = await Promise.all([this.#packageRepository.rootItems(), this.#packageRepository.migrations()]);

		const [package$, migration$] = data;

		this.observe(observeMultiple([package$, migration$]), ([packages, migrations]) => {
			this._installedPackages = packages.map((pkg) => {
				const migration = migrations.find((m) => m.packageName === pkg.name);
				if (migration) {
					// Remove that migration from the list
					migrations = migrations.filter((m) => m.packageName !== pkg.name);
				}

				return {
					...pkg,
					hasPendingMigrations: migration?.hasPendingMigrations ?? false,
				};
			});

			this._migrationPackages = migrations.map((m) => ({
				name: m.packageName,
				hasPendingMigrations: m.hasPendingMigrations ?? false,
				extensions: [],
			}));
		});
	}

	override render() {
		if (!this._installedPackages.length) return this.#renderNoPackages();
		return html`${this.#renderCustomMigrations()} ${this.#renderInstalled()} `;
	}

	#renderNoPackages() {
		return html`
			<div class="no-packages">
				<h2><strong>${this.localize.term('packager_noPackages')}</strong></h2>
				<p>${unsafeHTML(this.localize.term('packager_noPackagesDescription'))}</p>
			</div>
		`;
	}

	#renderInstalled() {
		return html`
			<uui-box headline=${this.localize.term('packager_installedPackages')} style="--uui-box-default-padding:0">
				<uui-ref-list>
					${repeat(
						this._installedPackages,
						(item) => item.name,
						(item) => html`
							<umb-installed-packages-section-view-item
								.name=${item.name}
								.version=${item.version}
								.hasPendingMigrations=${item.hasPendingMigrations}>
							</umb-installed-packages-section-view-item>
						`,
					)}
				</uui-ref-list>
			</uui-box>
		`;
	}

	#renderCustomMigrations() {
		if (!this._migrationPackages.length) return nothing;
		return html`
			<uui-box headline="Migrations" style="--uui-box-default-padding:0">
				<uui-ref-list>
					${repeat(
						this._migrationPackages,
						(item) => item.name,
						(item) => html`
							<umb-installed-packages-section-view-item
								custom-icon="icon-sync"
								.name=${item.name}
								.version=${item.version}
								.hasPendingMigrations=${item.hasPendingMigrations}>
							</umb-installed-packages-section-view-item>
						`,
					)}
				</uui-ref-list>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
			uui-box {
				margin-top: var(--uui-size-space-5);
				padding-bottom: var(--uui-size-space-1);
			}

			umb-installed-packages-section-view-item {
				padding: var(--uui-size-space-3) 0 var(--uui-size-space-2);
			}

			umb-installed-packages-section-view-item:not(:first-child) {
				border-top: 1px solid var(--uui-color-border, #d8d7d9);
			}

			.no-packages {
				display: flex;
				justify-content: space-around;
				flex-direction: column;
				align-items: center;
			}
		`,
	];
}

export default UmbInstalledPackagesSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-installed-packages-section-view': UmbInstalledPackagesSectionViewElement;
	}
}
