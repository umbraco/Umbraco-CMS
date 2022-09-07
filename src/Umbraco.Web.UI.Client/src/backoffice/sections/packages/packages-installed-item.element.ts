import { html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { firstValueFrom, map } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { createExtensionElement, UmbExtensionRegistry } from '../../../core/extension';

import type { ManifestPackageView, PackageInstalled } from '../../../core/models';
import type { UmbModalService } from '../../../core/services/modal';

@customElement('umb-packages-installed-item')
export class UmbPackagesInstalledItem extends UmbContextConsumerMixin(LitElement) {
	@property({ type: Object })
	package!: PackageInstalled;

	@state()
	private _packageView?: ManifestPackageView;

	private _umbExtensionRegistry?: UmbExtensionRegistry;
	private _umbModalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (umbExtensionRegistry: UmbExtensionRegistry) => {
			this._umbExtensionRegistry = umbExtensionRegistry;

			this.findPackageView(this.package.alias);
		});

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._umbModalService = modalService;
		});
	}

	private async findPackageView(alias: string) {
		const observable = this._umbExtensionRegistry
			?.extensionsOfType('packageView')
			.pipe(map((e) => e.filter((m) => m.meta.packageAlias === alias)));

		if (!observable) {
			return;
		}

		const views = await firstValueFrom(observable);
		if (!views.length) {
			return;
		}

		this._packageView = views[0];
	}

	render() {
		return html`
			<uui-ref-node-package name=${this.package.name} version=${this.package.version} @open=${this._onClick}>
				<uui-action-bar slot="actions">
					${this._packageView
						? html`<uui-button
								look="primary"
								color="positive"
								@click=${this._onConfigure}
								label="Configure"></uui-button>`
						: nothing}
				</uui-action-bar>
			</uui-ref-node-package>
		`;
	}

	private async _onConfigure() {
		if (!this._packageView) {
			console.warn('Tried to configure package without view');
			return;
		}

		const element = await createExtensionElement(this._packageView);

		if (!element) {
			console.warn('Failed to create package view element');
			return;
		}

		this._umbModalService?.open(element, { data: this.package, size: 'small', type: 'sidebar' });
	}

	private _onClick() {
		window.history.pushState({}, '', `/section/packages/details/${this.package.id}`);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-installed-item': UmbPackagesInstalledItem;
	}
}
