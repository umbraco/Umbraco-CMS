import type { ManifestDashboardApp, UmbDashboardAppSize } from '../app/dashboard-app.extension.js';
import { UMB_DASHBOARD_APP_PICKER_MODAL } from '../app/picker/picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbLitElement {
	#defaultSize: UmbDashboardAppSize = 'small';

	#sizeMap = new Map<UmbDashboardAppSize, UmbDashboardAppSize>([
		['small', 'small'],
		['medium', 'medium'],
		['large', 'large'],
	]);

	#extensionsController?: UmbExtensionsElementInitializer<UmbExtensionManifest, 'dashboardApp', ManifestDashboardApp>;

	@state()
	_appElements: Array<any> = [];

	@state()
	_appUniques: Array<string> = [];

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
						controller.component.setAttribute('size', `${size}`);
						controller.component.manifest = controller.manifest;
						return html`${controller.component}`;
					} else {
						return html`<uui-box>Not Found</uui-box>`;
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
				<div id="grid-container">
					${repeat(
						this._appElements,
						(element) => element,
						(element) => element,
					)}
				</div>
			</section>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#content {
				padding: var(--uui-size-layout-1);
			}

			#grid-container {
				display: grid;
				grid-template-columns: repeat(4, 1fr);
				grid-auto-rows: 225px;
				margin: calc(var(--uui-size-space-3) * -1);
				margin-bottom: 20px;

				[size='small'] {
					grid-column: span 1;
					grid-row: span 1;
					margin: var(--uui-size-space-3);
				}

				[size='medium'] {
					grid-column: span 2;
					grid-row: span 2;
					margin: var(--uui-size-space-3);
				}

				[size='large'] {
					grid-column: span 2;
					grid-row: span 3;
					margin: var(--uui-size-space-3);
				}
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
