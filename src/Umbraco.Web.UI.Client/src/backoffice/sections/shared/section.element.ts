import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, map, switchMap, EMPTY, of } from 'rxjs';
import { UmbSectionContext } from '../section.context';
import { UmbEditorEntityElement } from '../../editors/shared/editor-entity/editor-entity.element';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { createExtensionElement, UmbExtensionRegistry } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTree, ManifestSectionView } from '@umbraco-cms/models';

import './section-trees/section-trees.element.ts';
import '../shared/section-views/section-views.element.ts';

@customElement('umb-section')
export class UmbSectionElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			#header {
				display: flex;
				gap: 16px;
				align-items: center;
				min-height: 60px;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
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

	@state()
	private _views: Array<ManifestSectionView> = [];

	private _entityStore?: UmbEntityStore;

	private _sectionContext?: UmbSectionContext;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;
	private _viewsSubscription?: Subscription;

	constructor() {
		super();

		this.consumeAllContexts(['umbExtensionRegistry', 'umbSectionContext', 'umbEntityStore'], (instances) => {
			this._extensionRegistry = instances['umbExtensionRegistry'];
			this._sectionContext = instances['umbSectionContext'];
			this._entityStore = instances['umbEntityStore'];

			this._observeTrees();
			this._observeViews();
		});
	}

	private _observeTrees() {
		if (!this._sectionContext || !this._extensionRegistry || !this._entityStore) return;

		this._treesSubscription?.unsubscribe();

		this._treesSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionRegistry
							?.extensionsOfType('tree')
							.pipe(map((trees) => trees.filter((tree) => tree.meta.sections.includes(section.alias)))) ?? of([])
					);
				})
			)
			.subscribe((trees) => {
				this._trees = trees;
				if (this._trees.length === 0) return;
				this._createTreeRoutes();
			});
	}

	private _createTreeRoutes() {
		const treeRoutes =
			this._trees?.map(() => {
				return {
					path: `:entityType/:key`,
					component: () => import('../../editors/shared/editor-entity/editor-entity.element'),
					setup: (component: UmbEditorEntityElement, info: any) => {
						component.entityKey = info.match.params.key;
						component.entityType = info.match.params.entityType;
					},
				};
			}) ?? [];

		this._routes = [
			{
				path: 'dashboard',
				component: () => import('./section-dashboards/section-dashboards.element'),
			},
			...treeRoutes,
			{
				path: '**',
				redirectTo: 'dashboard',
			},
		];
	}

	private _observeViews() {
		if (!this._sectionContext || !this._extensionRegistry || !this._entityStore) return;

		this._viewsSubscription?.unsubscribe();

		this._viewsSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionRegistry
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
			)
			.subscribe((views) => {
				this._views = views;
				if (this._views.length === 0) return;
				this._createViewRoutes();
			});
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

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._viewsSubscription?.unsubscribe();
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
				<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
			</umb-section-main>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section': UmbSectionElement;
	}
}
