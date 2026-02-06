import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import type { UmbBlockWorkspaceViewEditTabElement } from './block-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, property, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeContainerMergedModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type {
	UmbRoute,
	UmbRouterSlotChangeEvent,
	UmbRouterSlotInitEvent,
	PageComponent,
} from '@umbraco-cms/backoffice/router';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UmbViewController, UMB_VIEW_CONTEXT } from '@umbraco-cms/backoffice/view';

@customElement('umb-block-workspace-view-edit')
export class UmbBlockWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@property({ attribute: false })
	public set manifest(value: ManifestWorkspaceView | undefined) {
		this.#managerName = (value?.meta as any).blockElementManagerName ?? 'content';
		this.#setStructureManager();
	}
	public get manifest(): ManifestWorkspaceView | undefined {
		return;
	}
	#managerName?: UmbBlockWorkspaceElementManagerNames;
	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);
	#viewContext?: typeof UMB_VIEW_CONTEXT.TYPE;

	@state()
	private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	private _tabs?: Array<UmbPropertyTypeContainerMergedModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	@state()
	private _hintMap: Map<string | null, UmbVariantHint> = new Map();

	#tabViewContexts: Array<UmbViewController> = [];

	constructor() {
		super();

		this.consumeContext(UMB_VIEW_CONTEXT, (context) => {
			this.#viewContext = context;
			this.#tabViewContexts.forEach((view) => {
				view.inheritFrom(this.#viewContext);
			});
		});

		this.#tabsStructureHelper.setIsRoot(true);
		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.observe(
			this.#tabsStructureHelper.childContainers,
			(tabs) => {
				this._tabs = tabs;
				this.#createRoutes();
			},
			null,
		);

		this.observe(
			this.#tabsStructureHelper.hasProperties,
			(hasRootProperties) => {
				this._hasRootProperties = hasRootProperties;
				this.#createRoutes();
			},
			'observeRootProperties',
		);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this.#setStructureManager();
		});
	}

	async #setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		const blockManager = this.#blockWorkspace[this.#managerName];
		this.#tabsStructureHelper.setStructureManager(blockManager.structure);

		this.observe(
			this.#blockWorkspace.variantId,
			(variantId) => {
				if (variantId) {
					// Create Data Set & setup Validation Context:
					blockManager.setup(this, variantId);
				}
			},
			'observeVariantId',
		);

		this.observe(
			await this.#blockWorkspace![this.#managerName!].structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this.#createRoutes();
			},
			'observeGroups',
		);
	}

	#createRoutes() {
		if (!this._tabs || !this.#blockWorkspace) return;
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				const path = `tab/${encodeFolderName(tabName)}`;
				routes.push({
					path,
					component: () => import('./block-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
						(component as UmbBlockWorkspaceViewEditTabElement).containerId = tab.ids[0];
						this.#provideViewContext(path, component);
					},
				});
				this.#createViewContext(path, tabName);
			});
		}

		if (this._hasRootGroups || this._hasRootProperties) {
			routes.push({
				path: '',
				component: () => import('./block-workspace-view-edit-tab.element.js'),
				setup: (component) => {
					(component as UmbBlockWorkspaceViewEditTabElement).managerName = this.#managerName;
					(component as UmbBlockWorkspaceViewEditTabElement).containerId = null;
					this.#provideViewContext(null, component);
				},
			});
			this.#createViewContext(null, '#general_generic');
		}

		if (routes.length !== 0) {
			if (!this._hasRootGroups) {
				routes.push({
					path: '',
					pathMatch: 'full',
					redirectTo: routes[0]?.path,
				});
			}
			routes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		this._routes = routes;
	}

	#createViewContext(viewAlias: string | null, tabName: string) {
		if (!this.#tabViewContexts.find((context) => context.viewAlias === viewAlias)) {
			const view = new UmbViewController(this, viewAlias);
			this.#tabViewContexts.push(view);

			if (viewAlias === null) {
				// for the root tab, we need to filter hints, so in this case we do accept everything that is not in a tab: [NL]
				view.hints.setPathFilter((paths) => paths[0].includes('tab/') === false);
			}

			view.setTitle(tabName);
			view.inheritFrom(this.#viewContext);

			this.observe(
				view.firstHintOfVariant,
				(hint) => {
					if (hint) {
						this._hintMap.set(viewAlias, hint);
					} else {
						this._hintMap.delete(viewAlias);
					}
					this.requestUpdate('_hintMap');
				},
				'umbObserveState_' + viewAlias,
			);
		}
	}

	#currentProvidedView?: UmbViewController;

	#provideViewContext(viewAlias: string | null, component: PageComponent) {
		const view = this.#tabViewContexts.find((context) => context.viewAlias === viewAlias);
		if (this.#currentProvidedView === view) {
			return;
		}
		this.#currentProvidedView?.unprovide();
		if (!view) {
			throw new Error(`View context with alias ${viewAlias} not found`);
		}
		this.#currentProvidedView = view;
		// ViewAlias null is only for the root tab, therefor we can implement this hack.
		if (viewAlias === null) {
			// Specific hack for the Generic tab to only show its name if there are other tabs.
			view.setTitle(this._tabs && this._tabs?.length > 0 ? '#general_generic' : undefined);
		}
		view.provideAt(component as any);
	}

	override render() {
		if (!this._routes || !this._tabs) return;
		return html`
			<umb-body-layout header-fit-height>
				${this._routerPath &&
				(this._tabs.length > 1 || (this._tabs.length === 1 && (this._hasRootGroups || this._hasRootProperties)))
					? html` <uui-tab-group slot="header">
							${(this._hasRootGroups || this._hasRootProperties) && this._tabs.length > 0
								? this.#renderTab(null, '#general_generic')
								: nothing}
							${repeat(
								this._tabs,
								(tab) => tab.name,
								(tab, index) => {
									const path = 'tab/' + encodeFolderName(tab.name || '');
									return this.#renderTab(path, tab.name, index);
								},
							)}
						</uui-tab-group>`
					: nothing}

				<umb-router-slot
					.routes=${this._routes}
					@init=${(event: UmbRouterSlotInitEvent) => {
						this._routerPath = event.target.absoluteRouterPath;
					}}
					@change=${(event: UmbRouterSlotChangeEvent) => {
						this._activePath = event.target.absoluteActiveViewPath || '';
					}}>
				</umb-router-slot>
			</umb-body-layout>
		`;
	}

	#renderTab(path: string | null, name: string, index = 0) {
		const hint = this._hintMap.get(path);
		const fullPath = this._routerPath + '/' + (path ? path : '');
		const active =
			fullPath === this._activePath ||
			(!this._hasRootGroups && index === 0 && this._routerPath + '/' === this._activePath) ||
			(this._hasRootGroups && index === 0 && path === null && this._routerPath + '/' === this._activePath);
		return html`<uui-tab
			label=${this.localize.string(name ?? '#general_unnamed')}
			.active=${active}
			href=${fullPath}
			data-mark="content-tab:${path ?? 'root'}"
			>${hint && !active
				? html`<umb-badge slot="extra" .color=${hint.color ?? 'default'} ?attention=${hint.color === 'invalid'}
						>${hint.text}</umb-badge
					>`
				: nothing}</uui-tab
		>`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
				--uui-tab-background: var(--uui-color-surface);
			}
		`,
	];
}

export default UmbBlockWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit': UmbBlockWorkspaceViewEditElement;
	}
}
