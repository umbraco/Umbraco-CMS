import { UMB_DASHBOARD_APP_PICKER_MODAL } from '../app/constants.js';
import type { ManifestDashboardApp } from '../app/types.js';
import type { DashboardAppInstance, UserDashboardAppConfiguration } from './types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat, styleMap, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	loadManifestElement,
	UmbExtensionsElementInitializer,
	type PermittedControllerType,
	type UmbExtensionElementInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UUIBlinkAnimationValue } from '@umbraco-cms/backoffice/external/uui';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UserDataService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbLitElement {
	#defaultSize = 'small';

	#sizeMap = new Map([
		['small', 'small'],
		['medium', 'medium'],
		['large', 'large'],
	]);

	#gridSizeMap = new Map([
		['small', { columns: 1, rows: 1 }],
		['medium', { columns: 2, rows: 2 }],
		['large', { columns: 2, rows: 2 }],
	]);

	#sorter?: UmbSorterController<DashboardAppInstance>;

	#extensionsController?: UmbExtensionsElementInitializer<UmbExtensionManifest, 'dashboardApp', ManifestDashboardApp>;

	@state()
	_editMode = false;

	@state()
	_apps: Array<DashboardAppInstance> = [];

	@state()
	_appUniques: Array<string> = [];

	@state()
	_availableApps: PermittedControllerType<
		UmbExtensionElementInitializer<ManifestDashboardApp, any, ManifestDashboardApp, HTMLElement | undefined>
	>[] = [];

	_userDataKey: string | undefined;

	constructor() {
		super();

		this.#sorter = new UmbSorterController<DashboardAppInstance>(this, {
			itemSelector: '.dashboard-app',
			containerSelector: '.grid-container',
			getUniqueOfElement: (element) => element.getAttribute('data-sorter-id'),
			getUniqueOfModel: (modelEntry) => modelEntry.unique,
			onChange: ({ model }) => {
				const oldValue = this._apps;
				this._apps = model;
				this.requestUpdate('_appElements', oldValue);
			},
		});

		this.#sorter.setModel(this._apps);
		this.#sorter.disable();

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
			undefined,
			(extensionControllers) => {
				this._availableApps = extensionControllers;

				this.#load();
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
		);
	}

	async #drawDashboardApps(config: UserDashboardAppConfiguration) {
		const apps = [...this._apps];

		if (config.apps.length == 0) {
			//TODO: Draw all?
		} else {
			for (const configuredApp of config.apps) {
				const controller = this._availableApps.find((x) => x.alias == configuredApp.alias);
				if (!controller) return undefined;

				const size = this.#sizeMap.get(controller.manifest.meta?.size) ?? this.#defaultSize;
				const headline = controller.manifest?.meta?.headline
					? this.localize.string(controller.manifest?.meta?.headline)
					: undefined;

				const gridSize = this.#gridSizeMap.get(size)!;

				const elementPropsValue = controller.manifest.element ?? controller.manifest.js;
				const appElementCtor = await loadManifestElement(elementPropsValue!)!; //TODO: Validate

				if (!appElementCtor) return;

				apps.push({
					unique: UmbId.new(),
					alias: controller.alias,
					columns: gridSize.columns,
					rows: gridSize?.rows,
					headline: headline,
					component: new appElementCtor(),
				});
			}
		}

		this._apps = apps;
	}

	async #openAppPicker() {
		const value = await umbOpenModal(this, UMB_DASHBOARD_APP_PICKER_MODAL, {
			data: {
				multiple: true,
			},
			value: {
				selection: [],
			},
		}).catch(() => undefined);

		if (value) {
			const apps = [...this._apps];

			for (const alias of value.selection) {
				const controller = this._availableApps.find((x) => x.alias == alias);
				if (!controller) return undefined;

				const size = this.#sizeMap.get(controller.manifest.meta?.size) ?? this.#defaultSize;
				const headline = controller.manifest?.meta?.headline
					? this.localize.string(controller.manifest?.meta?.headline)
					: undefined;

				const gridSize = this.#gridSizeMap.get(size)!;

				const elementPropsValue = controller.manifest.element ?? controller.manifest.js;
				const appElementCtor = await loadManifestElement(elementPropsValue!)!; //TODO: Validate

				if (!appElementCtor) return;

				apps.push({
					unique: UmbId.new(),
					alias: controller.alias,
					columns: gridSize.columns,
					rows: gridSize?.rows,
					headline: headline,
					component: new appElementCtor(),
				});
			}

			this._apps = apps;

			this.#save();
		}
	}

	async #save() {
		const config = {
			apps: this._apps.map((x) => {
				return { alias: x.alias };
			}),
		} as UserDashboardAppConfiguration;

		if (this._userDataKey) {
			await tryExecute(
				this,
				UserDataService.putUserData({
					body: {
						key: this._userDataKey,
						group: 'test',
						identifier: '',
						value: JSON.stringify(config),
					},
				}),
			);
		} else {
			await tryExecute(
				this,
				UserDataService.postUserData({
					body: {
						group: 'test',
						identifier: '',
						value: JSON.stringify(config),
					},
				}),
			);
		}
	}

	async #load() {
		const res = await tryExecute(
			this,
			UserDataService.getUserData({
				query: {
					groups: ['test'],
				},
			}),
		);

		if (res.data.items.length == 0) return;

		this._userDataKey = res.data.items[0].key;

		const userConfig = JSON.parse(res.data.items[0].value);

		this.#drawDashboardApps(userConfig);
	}

	async #enterEditMode() {
		this.#sorter?.setModel(this._apps);
		this.#sorter?.enable();
		this._editMode = true;
	}

	async #leaveEditMode() {
		this.#sorter?.disable();
		this._editMode = false;
		this.#save();
	}

	async #remove(elementKey: string) {
		this._apps = this._apps.filter((x) => x.unique != elementKey);
	}

	override render() {
		return html`
			<section id="content">
				<div class="main-actions">
					${when(
						this._editMode,
						() =>
							html`<uui-button @click=${this.#leaveEditMode}>Done <umb-icon name="icon-done"></umb-icon></uui-button>`,
						() =>
							html`<uui-button
									id="action-button"
									data-mark="workspace:action-menu-button"
									popovertarget="workspace-entity-action-menu-popover"
									label=${this.localize.term('general_actions')}>
									<uui-symbol-more></uui-symbol-more>
								</uui-button>
								<uui-popover-container id="workspace-entity-action-menu-popover" placement="bottom-end">
									<umb-popover-layout>
										<uui-scroll-container>
											<uui-menu-item @click=${this.#openAppPicker} label="Add">
												<umb-icon slot="icon" name="icon-add"></umb-icon>
											</uui-menu-item>
											<uui-menu-item @click=${this.#enterEditMode} label="Edit">
												<umb-icon slot="icon" name="icon-edit"></umb-icon>
											</uui-menu-item>
										</uui-scroll-container>
									</umb-popover-layout>
								</uui-popover-container>`,
					)}
				</div>
				<div class="grid-container">
					${repeat(
						this._apps,
						(element) => element.unique,
						(element) =>
							html` <div
								style=${styleMap({ gridColumn: `span ${element.columns}`, gridRow: `span ${element.rows}` })}
								class="dashboard-app"
								data-sorter-id=${element.unique}>
								${when(
									this._editMode,
									() =>
										html`<uui-button color="danger" compact @click=${() => this.#remove(element.unique)}
											><umb-icon name="icon-trash"></umb-icon
										></uui-button>`,
								)}
								${this.#renderComponent(element)}
							</div>`,
					)}
				</div>
			</section>
		`;
	}

	#renderComponent(element: DashboardAppInstance) {
		// TODO: Hacky rendering of component and entity context
		const component = element.component;
		if (!component) throw new Error('Dashboard app component is not defined');
		const entityContext = new UmbEntityContext(component);
		entityContext.setEntityType('dashboardApp');
		entityContext.setUnique(element.unique);
		return html`${element.component}`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				container-type: inline-size;
				--uui-menu-item-flat-structure: 1;
			}

			#content {
				padding: var(--uui-size-layout-1);
				padding-top: var(--uui-size-space-3);
				container-type: inline-size;
			}

			.main-actions {
				display: flex;
				justify-content: flex-end;
			}

			uui-box div[slot='header-actions'] uui-button {
				font-size: 12px;
				--uui-button-height: auto;
			}

			uui-box {
				height: 100%;
				position: relative;
			}

			.grid-container {
				margin-top: var(--uui-size-space-3);
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
				position: relative;
				display: block;
				height: 100%;

				&::after {
					content: '';
					position: absolute;
					z-index: 1;
					pointer-events: none;
					inset: 0;
					border: 1px solid transparent;
					border-radius: var(--uui-border-radius);
					transition: border-color 240ms ease-in;
				}

				& > uui-button {
					position: absolute;
					top: 0;
					right: 0;
					z-index: 1;
				}

				&[drag-placeholder] {
					position: relative;
					display: block;
					--umb-block-grid-entry-actions-opacity: 0;

					&::after {
						display: block;
						border-width: 2px;
						border-color: var(--uui-color-interactive-emphasis);
						animation: ${UUIBlinkAnimationValue};
					}

					&::before {
						content: '';
						position: absolute;
						pointer-events: none;
						inset: 0;
						border-radius: var(--uui-border-radius);
						background-color: var(--uui-color-interactive-emphasis);
						opacity: 0.12;
					}

					& > * {
						transition: opacity 50ms 16ms;
						opacity: 0;
					}
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
