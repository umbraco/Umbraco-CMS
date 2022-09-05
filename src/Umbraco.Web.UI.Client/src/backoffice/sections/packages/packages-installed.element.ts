import { html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { until } from 'lit/directives/until.js';
import { firstValueFrom, map } from 'rxjs';

import { getPackagesInstalled } from '../../../core/api/fetcher';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';

import type { PackageInstalled } from '../../../core/models';
@customElement('umb-packages-installed')
export class UmbPackagesInstalled extends UmbContextConsumerMixin(LitElement) {
	@state()
	private _installedPackages: PackageInstalled[] = [];

	@state()
	private _errorMessage = '';

	private umbExtensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (umbExtensionRegistry: UmbExtensionRegistry) => {
			this.umbExtensionRegistry = umbExtensionRegistry;
		});
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

	private findPackageView(alias: string) {
		const observable = this.umbExtensionRegistry
			?.extensionsOfType('packageView')
			.pipe(map((e) => e.filter((m) => m.meta.packageAlias === alias)));
		return observable ? firstValueFrom(observable) : undefined;
	}

	async renderPackage(p: PackageInstalled) {
		const packageView = await this.findPackageView(p.alias);
		return html`
			<uui-ref-node-package name=${p.name} version=${p.version}>
				<uui-action-bar slot="actions">
					${packageView?.length ? html`<uui-button label="Configure"></uui-button>` : nothing}
				</uui-action-bar>
			</uui-ref-node-package>
		`;
	}

	render() {
		return html`
			<uui-box headline="Installed packages">
				${this._errorMessage ? html`<uui-error-message>${this._errorMessage}</uui-error-message>` : nothing}

				<uui-ref-list>
					${repeat(
						this._installedPackages,
						(item) => item.id,
						(item) => until(this.renderPackage(item), 'Loading...')
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
