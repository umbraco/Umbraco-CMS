import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbPackageRepository } from '../../../repository/package.repository';
import type { UmbPackage } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

import './installed-packages-section-view-item.element';

@customElement('umb-installed-packages-section-view')
export class UmbInstalledPackagesSectionView extends UmbLitElement {
	@state()
	private _installedPackages: UmbPackage[] = [];

	private repository: UmbPackageRepository;

	constructor() {
		super();

		this.repository = new UmbPackageRepository(this);
	}

	firstUpdated() {
		this._loadInstalledPackages();
	}

	/**
	 * Fetch the installed packages from the server
	 */
	private async _loadInstalledPackages() {
		const package$ = await this.repository.rootItems();
		package$.subscribe((packages) => {
			this._installedPackages = packages.filter((p) => !!p.name);
		});
	}

	render() {
		return html`<uui-box headline="Installed packages">
			<uui-ref-list>
				${repeat(
					this._installedPackages,
					(item) => item.name,
					(item) =>
						html`<umb-installed-packages-section-view-item .package=${item}></umb-installed-packages-section-view-item>`
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
