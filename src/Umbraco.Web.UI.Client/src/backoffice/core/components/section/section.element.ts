import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, switchMap, EMPTY, of } from 'rxjs';
import { IRoutingInfo } from 'router-slot';
import { UmbSectionContext } from './section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTree, ManifestSectionView, ManifestWorkspace } from '@umbraco-cms/models';

import './section-trees/section-trees.element.ts';
import './section-views/section-views.element.ts';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-section')
export class UmbSectionElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			#router-slot {
				overflow: auto;
				height: 100%;
			}
		`,
	];

	// TODO: make this code reusable across sections
	@state()
	private _routes: Array<any> = [];

	@state()
	private _trees: Array<ManifestTree> = [];

	private _workspaces: Array<ManifestWorkspace> = [];

	@state()
	private _views: Array<ManifestSectionView> = [];

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (instance) => {
			this._sectionContext = instance;

			// TODO: currently they don't corporate, as they overwrite each other...
			this._observeTrees();
			this._observeViews();
		});
	}

	private _observeTrees() {
		if (!this._sectionContext) return;

		this.observe<ManifestTree[]>(
			this._sectionContext?.data.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;
					return (
						umbExtensionsRegistry
							?.extensionsOfType('tree')
							.pipe(map((trees) => trees.filter((tree) => tree.meta.sections.includes(section.alias)))) ?? of([])
					);
				})
			),
			(trees) => {
				this._trees = trees;
				this._createTreeRoutes();
			}
		);

		this.observe<ManifestWorkspace[]>(umbExtensionsRegistry?.extensionsOfType('workspace'), (workspaceExtensions) => {
			this._workspaces = workspaceExtensions;
			this._createTreeRoutes();
		});
	}

	private _createTreeRoutes() {
		const routes: any[] = [
			{
				path: 'dashboard',
				component: () => import('./section-dashboards/section-dashboards.element'),
			},
		];

		// TODO: find a way to make this reuseable across:
		this._workspaces?.map((workspace: ManifestWorkspace) => {
			routes.push({
				path: `${workspace.meta.entityType}/:key`,
				component: () => createExtensionElement(workspace),
				setup: (component: Promise<HTMLElement>, info: IRoutingInfo) => {
					component.then((el: HTMLElement) => {
						(el as any).entityKey = info.match.params.key;
					});
				},
			});
			routes.push({
				path: workspace.meta.entityType,
				component: () => createExtensionElement(workspace),
			});
		});

		routes.push({
			path: '**',
			redirectTo: 'dashboard',
		});
		this._routes = routes;
	}

	private _observeViews() {
		if (!this._sectionContext) return;

		this.observe<ManifestSectionView[]>(
			this._sectionContext.data.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						umbExtensionsRegistry
							?.extensionsOfType('sectionView')
							.pipe(
								map((views) =>
									views
										.filter((view) => view.meta.sections.includes(section.alias))
										.sort((a, b) => b.meta.weight - a.meta.weight)
								)
							) ?? of([])
					);
				})
			),
			(views) => {
				this._views = views;
				if (this._views.length === 0) return;
				this._createViewRoutes();
			}
		);
	}

	private _createViewRoutes() {
		this._routes =
			this._views?.map((view) => {
				return {
					path: 'view/' + view.meta.pathname,
					component: () => createExtensionElement(view),
					setup: () => {
						this._sectionContext?.setActiveView(view);
					},
				};
			}) ?? [];

		this._routes.push({
			path: '**',
			redirectTo: 'view/' + this._views?.[0]?.meta.pathname,
		});
	}

	render() {
		return html`
			${this._trees.length > 0
				? html`
						<umb-section-sidebar>
							<umb-section-trees></umb-section-trees>
						</umb-section-sidebar>
				  `
				: nothing}
			<umb-section-main>
				${this._views.length > 0 ? html`<umb-section-views></umb-section-views>` : nothing}
				${this._routes.length > 0
					? html` <router-slot id="router-slot" .routes="${this._routes}"></router-slot> `
					: nothing}
				<slot></slot>
			</umb-section-main>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section': UmbSectionElement;
	}
}
