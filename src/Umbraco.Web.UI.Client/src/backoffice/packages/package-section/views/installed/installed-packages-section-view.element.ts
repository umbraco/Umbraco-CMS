import { html, css } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { combineLatest } from 'rxjs';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbPackageRepository } from '../../../repository/package.repository';
import { UmbLitElement } from '@umbraco-cms/element';
import type { UmbPackageWithMigrationStatus } from '@umbraco-cms/models';

import './installed-packages-section-view-item.element';

@customElement('umb-installed-packages-section-view')
export class UmbInstalledPackagesSectionView extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
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

	@state()
	private _installedPackages: UmbPackageWithMigrationStatus[] = [];

	@state()
	private _migrationPackages: UmbPackageWithMigrationStatus[] = [];

	#packageRepository: UmbPackageRepository;

	constructor() {
		super();
		this.#packageRepository = new UmbPackageRepository(this);
	}

	firstUpdated() {
		this._loadInstalledPackages();
	}

	/**
	 * Fetch the installed packages from the server
	 */
	private async _loadInstalledPackages() {
		const data = await Promise.all([this.#packageRepository.rootItems(), this.#packageRepository.migrations()]);

		const [package$, migration$] = data;

		combineLatest([package$, migration$]).subscribe(([packages, migrations]) => {
			this._installedPackages = packages.map((p) => {
				const migration = migrations.find((m) => m.packageName === p.name);
				if (migration) {
					// Remove that migration from the list
					migrations = migrations.filter((m) => m.packageName !== p.name);
				}

				return {
					...p,
					hasPendingMigrations: migration?.hasPendingMigrations ?? false,
				};
			});

			this._migrationPackages = [
				...migrations.map((m) => ({
					name: m.packageName,
					hasPendingMigrations: m.hasPendingMigrations ?? false,
				})),
			];
			/*this._installedPackages = [
				...this._installedPackages,
				...migrations.map((m) => ({
					name: m.packageName,
					hasPendingMigrations: m.hasPendingMigrations ?? false,
				})),
			];*/
		});
	}

	render() {
		if (this._installedPackages.length) return html`${this._renderCustomMigrations()} ${this._renderInstalled()} `;
		return html`<div class="no-packages">
			<h2><strong>No packages have been installed</strong></h2>
			<p>
				Browse through the available packages using the <strong>'Packages'</strong> icon in the top right of your screen
			</p>
		</div>`;
	}

	private _renderInstalled() {
		return html`<uui-box headline="Installed packages" style="--uui-box-default-padding:0">
			<uui-ref-list>
				${repeat(
					this._installedPackages,
					(item) => item.name,
					(item) => html`<umb-installed-packages-section-view-item
						.name=${item.name}
						.version=${item.version}
						.hasPendingMigrations=${item.hasPendingMigrations}></umb-installed-packages-section-view-item>`
				)}
			</uui-ref-list>
		</uui-box>`;
	}

	private _renderCustomMigrations() {
		if (!this._migrationPackages) return;
		return html`<uui-box headline="Migrations" style="--uui-box-default-padding:0">
			<uui-ref-list>
				${repeat(
					this._migrationPackages,
					(item) => item.name,
					(item) => html`<umb-installed-packages-section-view-item
						.name=${item.name}
						.version=${item.version}
						.customIcon="${'umb:sync'}"
						.hasPendingMigrations=${item.hasPendingMigrations}></umb-installed-packages-section-view-item>`
				)}
			</uui-ref-list>
		</uui-box>`;
	}
}

export default UmbInstalledPackagesSectionView;

declare global {
	interface HTMLElementTagNameMap {
		'umb-installed-packages-section-view': UmbInstalledPackagesSectionView;
	}
}
