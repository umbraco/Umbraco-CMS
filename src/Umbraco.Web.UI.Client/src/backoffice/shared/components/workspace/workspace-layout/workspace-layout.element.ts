import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state, query } from 'lit/decorators.js';
import { IRouterSlot, IRoutingInfo, isPathActive } from 'router-slot';
import { map } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';

import { createExtensionElement , umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import type {
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
	ManifestWorkspaceViewCollection,
} from '@umbraco-cms/models';

import '../../body-layout/body-layout.element';
import '../../extension-slot/extension-slot.element';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-workspace-layout
 * @description
 * @slot icon - Slot for rendering the icon
 * @slot name - Slot for rendering the name
 * @slot footer - Slot for rendering the workspace footer
 * @slot actions - Slot for rendering the workspace actions
 * @slot default - slot for main content
 * @export
 * @class UmbWorkspaceLayout
 * @extends {UmbLitElement}
 */
@customElement('umb-workspace-layout')
export class UmbWorkspaceLayout extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			uui-input {
				width: 100%;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}
			router-slot {
				height: 100%;
				flex: 0;
			}

			umb-extension-slot[slot='actions'] {
				display: flex;
				gap: 6px;
			}
		`,
	];

	@property()
	public headline = '';

	private _alias = '';
	/**
	 * Alias of the workspace. The Layout will render the workspace views that are registered for this workspace alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property()
	public get alias() {
		return this._alias;
	}
	public set alias(value) {
		const oldValue = this._alias;
		this._alias = value;
		if (oldValue !== this._alias) {
			this._observeWorkspaceViews();
			this.requestUpdate('alias', oldValue);
		}
	}

	@state()
	private _workspaceViews: Array<ManifestWorkspaceView | ManifestWorkspaceViewCollection> = [];

	@state()
	private _routes: any[] = [];


	@query('router-slot')
	private _routerSlotEl?: IRouterSlot;

	@state()
	private _routerFolder = '';




	// TODO: can this be a controller?:

	@state()
	private _activeView?: typeof this._workspaceViews[number];

	#listening = false;


	connectedCallback() {
		super.connectedCallback();
		if(this.#listening) {
			window.addEventListener("changestate", this._onRouteChanged);
			this.#listening = true;
		}
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		window.removeEventListener("changestate", this._onRouteChanged);
		this.#listening = false;
	}


	firstUpdated() {
		this._routerFolder = this._routerSlotEl?.constructAbsolutePath('') || '';
		this._onRouteChanged();
	}

	private _onRouteChanged = () => {
		console.log("_onRouteChanged", this._routerSlotEl);
		if(this._routerSlotEl) {
			this._workspaceViews.forEach(view => {
				if(isPathActive(this._routerSlotEl!.constructAbsolutePath('view/'+view.meta.pathname))) {
					this._activeView = view;
				}
			});
		}
	}


	private _observeWorkspaceViews() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfTypes<ManifestWorkspaceView>(['workspaceView', 'workspaceViewCollection'])
				.pipe(map((extensions) => extensions.filter((extension) => extension.meta.workspaces.includes(this.alias)))),
			(workspaceViews) => {
				this._workspaceViews = workspaceViews;
				this._createRoutes();
			}
		);
	}

	private _createRoutes() {
		this._routes = [];

		if (this._workspaceViews.length > 0) {
			this._routes = this._workspaceViews.map((view) => {
				return {
					path: `view/${view.meta.pathname}`,
					component: () => {
						if (view.type === 'workspaceViewCollection') {
							return import(
								'src/backoffice/shared/components/workspace/workspace-content/views/collection/workspace-view-collection.element'
							);
						}
						return createExtensionElement(view);
					},
					setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
						// When its using import, we get an element, when using createExtensionElement we get a Promise.
						(component as any).manifest = view;
						if ((component as any).then) {
							(component as any).then((el: any) => (el.manifest = view));
						}
					},
				};
			});

			this._routes.push({
				path: '**',
				redirectTo: `view/${this._workspaceViews[0].meta.pathname}`,
			});

		}
	}

	/*
	private _temp_isActive(view: any) {

		console.log("_temp_isActive??", this._routerSlotEl)
		if(this._routerSlotEl) {
			const viewPath = this._routerSlotEl?.constructAbsolutePath('view/'+view.meta.pathname);
			console.log("IsActive??", viewPath, isPathActive(viewPath))
			console.log(view.name, viewPath, isPathActive(viewPath))
			return isPathActive(viewPath);
		}
		return false;
	}
	*/

	private _renderTabs() {
		return html`
			${this._workspaceViews.length > 0
				? html`
						<uui-tab-group slot="tabs">
							${repeat(
								this._workspaceViews,
								(view) => view.alias,
								(view) => html`
										<uui-tab
											.label="${view.meta.label || view.name}"
											href="${this._routerFolder}/view/${view.meta.pathname}"
											?active="${view === this._activeView}">
											<uui-icon slot="icon" name="${view.meta.icon}"></uui-icon>
											${view.meta.label || view.name}
										</uui-tab>
									`
								)
							}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}

	render() {
		return html`
			<umb-body-layout .headline=${this.headline}>
				<slot name="header" slot="header"></slot>
				${this._renderTabs()}

				<router-slot .routes="${this._routes}"></router-slot>
				<slot></slot>

				<slot name="footer" slot="footer"></slot>
				<umb-extension-slot
					slot="actions"
					type="workspaceAction"
					.filter=${(extension: ManifestWorkspaceAction) =>
						extension.meta.workspaces.includes(this.alias)}></umb-extension-slot>
				<slot name="actions" slot="actions"></slot>
			</umb-body-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-layout': UmbWorkspaceLayout;
	}
}
