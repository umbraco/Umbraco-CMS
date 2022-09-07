import './packages-installed-item.element';

import { html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';

import { getPackagesInstalled } from '../../../core/api/fetcher';

import type { PackageInstalled } from '../../../core/models';

@customElement('umb-packages-installed')
export class UmbPackagesInstalled extends LitElement {
	@state()
	private _installedPackages: PackageInstalled[] = [];

	@state()
	private _errorMessage = '';

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();

		this._loadInstalledPackages();
	}

	/**
	 * Fetch the installed packages from the server
	 */
	private async _loadInstalledPackages() {
		this._errorMessage = '';

		try {
			const {
				data: { packages },
			} = await getPackagesInstalled({});
			this._installedPackages = packages;
		} catch (e) {
			if (e instanceof getPackagesInstalled.Error) {
				const error = e.getActualType();
				this._errorMessage = error.data.detail ?? 'An error occurred while loading the installed packages';
			}
		}
	}

	render() {
		return html`
			<uui-box headline="Installed packages">
				${this._errorMessage ? html`<uui-error-message>${this._errorMessage}</uui-error-message>` : nothing}

				<uui-ref-list>
					${repeat(
						this._installedPackages,
						(item) => item.id,
						(item) => html`<umb-packages-installed-item .package=${item}></umb-packages-installed-item>`
					)}
				</uui-ref-list>
			</uui-box>
		`;
	}
}

export default UmbPackagesInstalled;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-installed': UmbPackagesInstalled;
	}
}
