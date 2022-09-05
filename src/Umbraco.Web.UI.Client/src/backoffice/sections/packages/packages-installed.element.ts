import { html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getPackagesInstalled } from '../../../core/api/fetcher';
import { UmbContextConsumerMixin } from '../../../core/context';

import type { PackageInstalled } from '../../../core/models';

@customElement('umb-packages-installed')
export class UmbPackagesInstalled extends UmbContextConsumerMixin(LitElement) {
	@state()
	private _installedPackages: PackageInstalled[] = [];

	@state()
	private _errorMessage = '';

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
					${this._installedPackages.map(
						(p) => html` <uui-ref-node-package name=${p.name} version=${p.version}></uui-ref-node-package> `
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
