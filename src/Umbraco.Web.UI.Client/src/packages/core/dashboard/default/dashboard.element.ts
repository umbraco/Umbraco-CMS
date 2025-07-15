import type { ManifestDashboardApp } from '../dashboard-app.extension.js';
import { UMB_DASHBOARD_APP_PICKER_MODAL } from '../app/picker/picker-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, ifDefined, state, repeat, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbExtensionsElementInitializer,
	type UmbExtensionElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIBlinkAnimationValue, UUIBlinkKeyframes } from '@umbraco-cms/backoffice/external/uui';
import type { DashboardAppInstance } from './types.js';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbId } from '@umbraco-cms/backoffice/id';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbLitElement {
	#defaultSize = 'small';

	#sizeMap = new Map([
		['small', 'small'],
		['medium', 'medium'],
		['large', 'large'],
	]);

	#gridSizeMap = new Map([
		['small',{columns:1, rows: 1}],
		['medium',{columns:2, rows: 2}],
		['large',{columns:2, rows: 2}],
	]);

	#sorter? : UmbSorterController<DashboardAppInstance>;

	#extensionsController?: UmbExtensionsElementInitializer<UmbExtensionManifest, 'dashboardApp', ManifestDashboardApp>;

	@state()
	_appElements: Array<DashboardAppInstance> = [];

	@state()
	_appUniques: Array<string> = [];

	constructor() {
		super();

		this.#sorter = new UmbSorterController<DashboardAppInstance>(this, {
			itemSelector: '.dashboard-app',
			containerSelector: '.grid-container',
			getUniqueOfElement: (element) => element.getAttribute('data-sorter-id'),
			getUniqueOfModel: (modelEntry) => modelEntry.key,
			onChange: ({ model }) => {
				const oldValue = this._appElements;
				this._appElements = model;
				this.requestUpdate('_appElements', oldValue);
			},
		});

		this.#sorter.setModel(this._appElements);

		this.#observeDashboardApps();

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
			// If no _appUniques, return all
			(manifest) => this._appUniques.length == 0 || this._appUniques.includes(manifest.alias),
			(extensionControllers) => {

				let newAppElements : DashboardAppInstance[] = [];

				extensionControllers.forEach((controller)=>{

					if (controller.component && controller.manifest) {
						const size = this.#sizeMap.get(controller.manifest.meta?.size) ?? this.#defaultSize;
						const headline = controller.manifest?.meta?.headline
							? this.localize.string(controller.manifest?.meta?.headline)
							: undefined;

						let gridSize = this.#gridSizeMap.get(size)!;

						newAppElements.push({
							key : UmbId.new(),
							columns : gridSize.columns,
							rows : gridSize?.rows,
							headline : headline,
							component : controller.component,
						});


					} else {
						newAppElements.push({
							key : UmbId.new(),
							columns : 1,
							rows : 1,
							}
						);
					}
				});

				this._appElements = newAppElements;

				this.#sorter?.setModel(this._appElements);

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
						(element) => element.key,
						(element) =>
							html`
								<div
									style=${styleMap({gridColumn:`span ${element.columns}`,gridRow:`span ${element.rows}`})}
									class="dashboard-app"
									data-sorter-id=${element.key}>
									<uui-box headline=${element.headline ?? ""}>
										${element.component}
									</uui-box>
								</div>`,
					)}
				</div>
			</section>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				container-type: inline-size;
			}

			uui-box {
				height:100%;
				position:relative;
			}

			#content {
				padding: var(--uui-size-layout-1);
				container-type: inline-size;
			}

			.grid-container {
				margin-top:var(--uui-size-layout-1);
				display: grid;
				grid-template-columns: repeat(4, 1fr);
				grid-auto-rows: 225px;
				gap: 20px;
			}

			@container (inline-size < 900px) {
				.grid-container {
					grid-template-columns: repeat(3, 1fr);
				}
			}

			@container (inline-size < 601px) {
				.grid-container {
					grid-template-columns: repeat(1, 1fr);
				}
				.grid-container > * {
					grid-column: span 1 !important;
				}
			}

			.dashboard-app {
				position:relative;
				display:block;
				height:100%;
			}

			.dashboard-app::after {
				content: '';
				position: absolute;
				z-index: 1;
				pointer-events: none;
				inset: 0;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);

				transition: border-color 240ms ease-in;
			}

			.dashboard-app[drag-placeholder] {
				position: relative;
				display: block;
				--umb-block-grid-entry-actions-opacity: 0;
			}

			.dashboard-app[drag-placeholder]::after {
				display: block;
				border-width: 2px;
				border-color: var(--uui-color-interactive-emphasis);
				animation: ${UUIBlinkAnimationValue};
			}

			.dashboard-app[drag-placeholder]::before {
				content: '';
				position: absolute;
				pointer-events: none;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-interactive-emphasis);
				opacity: 0.12;
			}

			.dashboard-app[drag-placeholder] > * {
				transition: opacity 50ms 16ms;
				opacity: 0;
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
