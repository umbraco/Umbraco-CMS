import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { first, map } from 'rxjs';
import { UmbSectionContext } from '../section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type {
	ManifestDashboard,
	ManifestDashboardCollection,
	ManifestSection,
	ManifestWithMeta,
} from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-section-dashboards')
export class UmbSectionDashboardsElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
				width: 100%;
			}

			#tabs {
				background-color: var(--uui-color-surface);
				height: 70px;
			}

			#scroll-container {
				width: 100%;
				height: 100%;
			}

			#router-slot {
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				display: block;
			}
		`,
	];

	@state()
	private _dashboards: Array<ManifestDashboard | ManifestDashboardCollection> = [];

	@state()
	private _currentDashboardPathname = '';

	@state()
	private _routes: Array<any> = [];

	@state()
	private _currentSectionPathname = '';

	private _currentSectionAlias = '';
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (context: UmbSectionContext) => {
			this._sectionContext = context;
			this._observeSectionContext();
		});
	}

	private _observeSectionContext() {
		if (!this._sectionContext) return;

		this.observe<ManifestSection>(this._sectionContext.data.pipe(first()), (section) => {
			this._currentSectionAlias = section.alias;
			this._currentSectionPathname = section.meta.pathname;
			this._observeDashboards();
		});
	}

	private _observeDashboards() {
		if (!this._currentSectionAlias) return;

		this.observe<ManifestDashboard[]>(
			umbExtensionsRegistry
				?.extensionsOfTypes(['dashboard', 'dashboardCollection'])
				.pipe(
					map((extensions) =>
						extensions.filter((extension) =>
							(extension as ManifestWithMeta).meta.sections.includes(this._currentSectionAlias)
						)
					)
				),
			(dashboards) => {
				if (dashboards?.length === 0) return;
				this._dashboards = dashboards;
				this._createRoutes();
			}
		);
	}

	private _createRoutes() {
		this._routes = [];

		this._routes = this._dashboards.map((dashboard) => {
			return {
				path: `${dashboard.meta.pathname}`,
				component: () => {
					if (dashboard.type === 'dashboardCollection') {
						return import('src/backoffice/dashboards/collection/dashboard-collection.element');
					}
					return createExtensionElement(dashboard);
				},
				setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
					this._currentDashboardPathname = info.match.route.path;
					// When its using import, we get an element, when using createExtensionElement we get a Promise.
					(component as any).manifest = dashboard;
					if ((component as any).then) {
						(component as any).then((el: any) => (el.manifest = dashboard));
					}
				},
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._dashboards?.[0]?.meta.pathname,
		});
	}

	private _renderNavigation() {
		return html`
			${this._dashboards?.length > 1
				? html`
						<uui-tab-group id="tabs">
							${this._dashboards.map(
								(dashboard) => html`
									<uui-tab
										href="${`section/${this._currentSectionPathname}/dashboard/${dashboard.meta.pathname}`}"
										label=${dashboard.meta.label || dashboard.name}
										?active="${dashboard.meta.pathname === this._currentDashboardPathname}"></uui-tab>
								`
							)}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}

	render() {
		return html`
			${this._renderNavigation()}
			<uui-scroll-container id="scroll-container">
				<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
			</uui-scroll-container>
		`;
	}
}

export default UmbSectionDashboardsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-dashboards': UmbSectionDashboardsElement;
	}
}
