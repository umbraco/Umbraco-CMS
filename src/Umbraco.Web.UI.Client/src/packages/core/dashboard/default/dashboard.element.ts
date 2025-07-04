import type { ManifestDashboardApp } from '../dashboard-app.extension.js';
import { UMB_DASHBOARD_APP_PICKER_MODAL } from '../app/picker/picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, ifDefined, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbExtensionsElementInitializer,
	type UmbExtensionElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbLitElement {
	#defaultSize = 'small';

	#sizeMap = new Map([
		['small', 'small'],
		['medium', 'medium'],
		['large', 'large'],
	]);

	#extensionsController?: UmbExtensionsElementInitializer<UmbExtensionManifest, 'dashboardApp', ManifestDashboardApp>;

	@state()
	_appElements: Array<any> = [];

	@state()
	_appUniques: Array<string> = [];

	constructor() {
		super();
	}

	#observeDashboardApps(): void {
		this.#extensionsController?.destroy();
		this.#extensionsController = new UmbExtensionsElementInitializer<
			UmbExtensionManifest,
			'dashboardApp',
			ManifestDashboardApp
		>(
			this,
			umbExtensionsRegistry,
			'dashboardApp',
			(manifest) => this._appUniques.includes(manifest.alias),
			(extensionControllers) => {
				this._appElements = extensionControllers.map((controller) => {
					if (controller.component && controller.manifest) {
						const size = this.#sizeMap.get(controller.manifest.meta?.size) ?? this.#defaultSize;
						const headline = controller.manifest?.meta?.headline
							? this.localize.string(controller.manifest?.meta?.headline)
							: undefined;

						return html`<uui-box part="umb-dashboard-app-${size}" headline=${ifDefined(headline)}
							>${controller.component}</uui-box
						>`;
					} else {
						return html`<uui-box part="umb-dashboard-app-${this.#defaultSize}">Not Found</uui-box>`;
					}
				});
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
		);
	}

	async #openAppPicker() {
		const value = await umbOpenModal(this, UMB_DASHBOARD_APP_PICKER_MODAL, {
			data: {
				multiple: true,
			},
			value: {
				selection: this._appUniques,
			},
		}).catch(() => undefined);

		if (value) {
			this._appUniques = value.selection.filter((item) => item !== null) as Array<string>;
			this.#observeDashboardApps();
		}
	}

	override render() {
		return html`
			<section id="content">
				<uui-button look="placeholder" @click=${this.#openAppPicker}>Add</uui-button>
				<div class="grid-container">
					${repeat(
						this._appElements,
						(element) => element,
						(element) => element,
					)}
				</div>
				<umb-extension-slot
					type="dashboardApp"
					.renderMethod=${this.#extensionSlotRenderMethod}
					class="grid-container"></umb-extension-slot>
			</section>
		`;
	}

	#extensionSlotRenderMethod = (ext: UmbExtensionElementInitializer<ManifestDashboardApp>) => {
		if (ext.component && ext.manifest) {
			const size = this.#sizeMap.get(ext.manifest.meta?.size) ?? this.#defaultSize;
			const headline = ext.manifest?.meta?.headline ? this.localize.string(ext.manifest?.meta?.headline) : undefined;
			return html`<uui-box part="umb-dashboard-app-${size}" headline=${ifDefined(headline)}>${ext.component}</uui-box>`;
		}

		return nothing;
	};

	static override styles = [
		UmbTextStyles,
		css`
			#content {
				padding: var(--uui-size-layout-1);
			}

			.grid-container {
				display: grid;
				grid-template-columns: repeat(4, 1fr);
				//grid-template-rows: repeat(100, 225px);
				margin: calc(var(--uui-size-space-3) * -1);
				margin-bottom: 20px;
			}

			umb-extension-slot::part(umb-dashboard-app-small) {
				grid-column: span 1;
				grid-row: span 1;
				margin: var(--uui-size-space-3);
			}

			umb-extension-slot::part(umb-dashboard-app-medium) {
				grid-column: span 2;
				grid-row: span 2;
				margin: var(--uui-size-space-3);
			}

			umb-extension-slot::part(umb-dashboard-app-large) {
				grid-column: span 2;
				grid-row: span 3;
				margin: var(--uui-size-space-3);
			}
		`,
	];
}

export { UmbDashboardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-dashboard']: UmbDashboardElement;
	}
}
