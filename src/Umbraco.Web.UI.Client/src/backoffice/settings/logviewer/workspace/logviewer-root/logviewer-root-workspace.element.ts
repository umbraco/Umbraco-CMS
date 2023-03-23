import './components';
import { map } from 'rxjs';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { repeat } from 'lit/directives/repeat.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry, createExtensionElement } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestWorkspaceView, ManifestWorkspaceViewCollection } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbRouterSlotInitEvent, UmbRouterSlotChangeEvent } from '@umbraco-cms/internal/router';

//TODO make uui-input accept min and max values
@customElement('umb-logviewer-workspace')
export class UmbLogViewerWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;

				--umb-log-viewer-debug-color: var(--uui-color-default-emphasis);
				--umb-log-viewer-information-color: var(--uui-color-positive);
				--umb-log-viewer-warning-color: var(--uui-color-warning);
				--umb-log-viewer-error-color: var(--uui-color-danger);
				--umb-log-viewer-fatal-color: var(--uui-color-default);
				--umb-log-viewer-verbose-color: var(--uui-color-current);
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				align-items: center;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}
		`,
	];

	private _alias = 'Umb.Workspace.LogviewerRoot';

	@state()
	private _workspaceViews: Array<ManifestWorkspaceView | ManifestWorkspaceViewCollection> = [];

	@state()
	private _routes: any[] = [];

	@state()
	private _activePath?: string;

	@state()
	private _routerPath?: string;

	#logViewerContext = new UmbLogViewerWorkspaceContext(this);

	constructor() {
		super();
		this.#logViewerContext.init();
	}

	connectedCallback() {
		super.connectedCallback();
		this._observeWorkspaceViews();
		this.provideContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, this.#logViewerContext);
	}

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
	}

	private _observeWorkspaceViews() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfTypes<ManifestWorkspaceView>(['workspaceView'])
				.pipe(
					map((extensions) => extensions.filter((extension) => extension.conditions.workspaces.includes(this._alias)))
				),
			(workspaceViews) => {
				this._workspaceViews = workspaceViews;
				this._createRoutes();
			}
		);
	}

	create(): void {
		// Not relevant for this workspace
	}

	private _createRoutes() {
		this._routes = [];

		if (this._workspaceViews.length > 0) {
			this._routes = this._workspaceViews.map((view) => {
				return {
					path: `${view.meta.pathname}`,
					component: () => {
						return createExtensionElement(view);
					},
					setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
						// When its using import, we get an element, when using createExtensionElement we get a Promise.
						if ((component as any).then) {
							(component as any).then((el: any) => (el.manifest = view));
						} else {
							(component as any).manifest = view;
						}
					},
				};
			});

			this._routes.push({
				path: '**',
				redirectTo: `${this._workspaceViews[0].meta.pathname}`,
			});
		}
	}

	#renderRoutes() {
		return html`
			${this._routes.length > 0
				? html`
						<umb-router-slot
							id="router-slot"
							.routes="${this._routes}"
							@init=${(event: UmbRouterSlotInitEvent) => {
								this._routerPath = event.target.absoluteRouterPath;
							}}
							@change=${(event: UmbRouterSlotChangeEvent) => {
								this._activePath = event.target.localActiveViewPath;
							}}></umb-router-slot>
				  `
				: nothing}
		`;
	}

	#renderViews() {
		return html`
			${this._workspaceViews.length > 1
				? html`
						<uui-tab-group slot="tabs">
							${repeat(
								this._workspaceViews,
								(view) => view.alias,
								(view) => html`
									<uui-tab
										.label="${view.meta.label || view.name}"
										href="${this._routerPath}/${view.meta.pathname}"
										?active="${view.meta.pathname === this._activePath}">
										<uui-icon slot="icon" name="${view.meta.icon}"></uui-icon>
										${view.meta.label || view.name}
									</uui-tab>
								`
							)}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}

	render() {
		return html`
			<umb-body-layout>
				<div id="header" slot="header">
					<h3 id="headline">
						${this._activePath === 'overview' ? 'Log Overview for Selected Time Period' : 'Log search'}
					</h3>
				</div>
				${this.#renderViews()} ${this.#renderRoutes()}
				<slot></slot>
			</umb-body-layout>
		`;
	}
}

export default UmbLogViewerWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-workspace': UmbLogViewerWorkspaceElement;
	}
}
