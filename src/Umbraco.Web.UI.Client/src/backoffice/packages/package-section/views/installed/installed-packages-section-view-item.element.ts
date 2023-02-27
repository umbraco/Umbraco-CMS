import { html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { firstValueFrom, map } from 'rxjs';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

import type { ManifestPackageView, UmbPackage } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-installed-packages-section-view-item')
export class UmbInstalledPackagesSectionViewItemElement extends UmbLitElement {
	@property({ type: Object })
	package!: UmbPackage;

	@state()
	private _packageView?: ManifestPackageView;

	private _umbModalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._umbModalService = modalService;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		if (this.package.name?.length) {
			this.findPackageView(this.package.name);
		}
	}

	private async findPackageView(alias: string) {
		const observable = umbExtensionsRegistry
			?.extensionsOfType('packageView')
			.pipe(map((e) => e.filter((m) => m.meta.packageName === alias)));

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
			<uui-ref-node-package name=${ifDefined(this.package.name)} version=${ifDefined(this.package.version)}>
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installed-packages-section-view-item': UmbInstalledPackagesSectionViewItemElement;
	}
}
