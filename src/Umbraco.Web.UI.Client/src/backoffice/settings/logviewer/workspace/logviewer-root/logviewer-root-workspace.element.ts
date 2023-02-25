import '../donut-chart';
import { map } from 'rxjs';
import { css, html, nothing } from 'lit';
import { customElement, state, property } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry, createExtensionElement } from '@umbraco-cms/extensions-api';
import { ManifestWorkspaceView, ManifestWorkspaceViewCollection } from '@umbraco-cms/extensions-registry';
import { UmbRouterSlotInitEvent, UmbRouterSlotChangeEvent } from '@umbraco-cms/router';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../logviewer.context';

//TODO make uui-input accept min and max values
@customElement('umb-logviewer-workspace')
export class UmbLogViewerWorkspaceElement extends UmbLitElement {
	// render() {
	// 	return html`
	// 	<umb-body-layout headline="Log Overview for today">
	// 		<div id="logviewer-layout">
	// 			<div id="info">

	// 				<uui-box id="time-period" headline="Time Period">
	// 					<div id="date-input-container" @input=${this.#setDates}>
	// 						<uui-label for="start-date">From:</uui-label>
	// 						<input
	// 							id="start-date"
	// 							type="date"
	// 							label="From"
	// 							max="${this.today}"
	// 							.value=${this._startDate}>
	// 						</input>
	// 						<uui-label for="end-date">To: </uui-label>
	// 						<input
	// 							id="end-date"
	// 							type="date"
	// 							label="To"
	// 							max="${this.today}"
	// 							.value=${this._endDate}>
	// 						</input>
	// 					</div>
	// 				</uui-box>

	// 				<uui-box id="errors" headline="Number of Errors"></uui-box>

	// 				<uui-box id="level" headline="Log level"></uui-box>

	// 				<uui-box id="types" headline="Log types">
	// 					<div id="log-types-container">
	// 						<div id="legend">
	// 							<ul>
	// 								${
	// 									this._logLevelCount
	// 										? Object.keys(this._logLevelCount).map(
	// 												(level) =>
	// 													html`<li>
	// 														<button
	// 															@click=${(e: Event) => {
	// 																(e.target as HTMLElement)?.classList.toggle('active');
	// 																this.#setCountFilter(level);
	// 															}}>
	// 															<uui-icon
	// 																name="umb:record"
	// 																style="color: var(--umb-log-viewer-${level.toLowerCase()}-color);"></uui-icon
	// 															>${level}
	// 														</button>
	// 													</li>`
	// 										  )
	// 										: ''
	// 								}
	// 							</ul>
	// 						</div>
	// 						<umb-donut-chart>
	// 						${
	// 							this._logLevelCount
	// 								? this.logLevelCount.map(
	// 										([level, number]) =>
	// 											html`<umb-donut-slice
	// 												.name=${level}
	// 												.amount=${number}
	// 												.percent=${this.#calculatePercentage(number)}
	// 												.color="${`var(--umb-log-viewer-${level.toLowerCase()}-color)`}"></umb-donut-slice> `
	// 								  )
	// 								: ''
	// 						}
	// 						</umb-donut-chart>
	// 					</div>
	// 				</uui-box>
	// 			</div>

	// 			<div id="saved-searches-container">
	// 				<uui-box id="saved-searches" headline="Saved searches">
	// 					<ul>${this._savedSearches.map(this.#renderSearchItem)}</ul>
	// 				</uui-box>
	// 			</div>

	// 			<div id="common-messages-container">
	// 				<uui-box headline="Common Log Messages" id="saved-searches">
	// 					<p style="font-style: italic;">Total Unique Message types: ${this._messageTemplates?.total}</p>
	// 				<uui-table>
	// 				${
	// 					this._messageTemplates
	// 						? this._messageTemplates.items.map(
	// 								(template) =>
	// 									html`<uui-table-row
	// 										><uui-table-cell>${template.messageTemplate}</uui-table-cell>
	// 										<uui-table-cell>${template.count}</uui-table-cell>
	// 									</uui-table-row>`
	// 						  )
	// 						: ''
	// 				}
	// 		</uui-table>
	// 				<uui-button id="show-more-templates-btn" look="primary">Show more</uui-button>
	// 				</uui-box>
	// 			</div>
	// 		</div>
	// 	</umb-body-layout>`;
	// }

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
				align-items: center;
			}

			#router-slot {
				height: 100%;
			}
		`,
	];

	@property()
	public headline = 'Log Overview for Selected Time Period';

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

	connectedCallback() {
		super.connectedCallback();
		this._observeWorkspaceViews();
		this.provideContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, this.#logViewerContext);
	}

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
		console.log('Loading something from somewhere');
	}

	private _observeWorkspaceViews() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfTypes<ManifestWorkspaceView>(['workspaceView'])
				.pipe(map((extensions) => extensions.filter((extension) => extension.meta.workspaces.includes(this._alias)))),
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

	render() {
		return html`
			<umb-body-layout>
				<div id="header" slot="header">
					${this._activePath === 'search'
						? html`<a href="/section/settings/logviewer">
								<uui-button compact>
									<uui-icon name="umb:arrow-left"></uui-icon>
								</uui-button>
						  </a>`
						: ''}
					<h3 id="headline">
						${this._activePath === 'overview' ? 'Log Overview for Selected Time Period' : 'Log search'}
					</h3>
				</div>
				${this.#renderRoutes()}
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
