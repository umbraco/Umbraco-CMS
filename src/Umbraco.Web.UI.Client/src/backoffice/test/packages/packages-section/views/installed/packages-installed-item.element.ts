import { html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { firstValueFrom, map } from 'rxjs';

import type { UmbModalService } from '../../../../../../core/services/modal';
import { createExtensionElement } from '@umbraco-cms/extensions-api';

import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestPackageView } from '@umbraco-cms/models';

@customElement('umb-packages-installed-item')
export class UmbPackagesInstalledItem extends UmbContextConsumerMixin(LitElement) {
	@property({ type: Object })
	package!: any; // TODO: Use real type

	@state()
	private _packageView?: ManifestPackageView;

	private _umbModalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._umbModalService = modalService;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.findPackageView(this.package.alias);
	}

	private async findPackageView(alias: string) {
		const observable = umbExtensionsRegistry
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
		window.history.pushState({}, '', `/section/packages/view/installed/package/${this.package.id}`);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-installed-item': UmbPackagesInstalledItem;
	}
}
