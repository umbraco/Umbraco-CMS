import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, switchMap, EMPTY, of } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import { createExtensionElement, UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';
import type { ManifestTree, ManifestSectionView } from '../../../core/models';
import { UmbEditorEntityElement } from '../../editors/shared/editor-entity/editor-entity.element';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbObserverMixin } from '../../../core/observer';

import './section-trees/section-trees.element.ts';
import '../shared/section-views/section-views.element.ts';

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

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionsRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionsRegistry;
			this._observeTrees();
			this._observeViews();
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeTrees();
			this._observeViews();
		});

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			this._observeTrees();
			this._observeViews();
		});
	}

	private _observeTrees() {
		if (!this._sectionContext || !this._extensionRegistry || !this._entityStore) return;

		this.observe<ManifestTree[]>(
			this._sectionContext?.data.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionRegistry
							?.extensionsOfType('tree')
							.pipe(map((trees) => trees.filter((tree) => tree.meta.sections.includes(section.alias)))) ?? of([])
					);
				})
			),
			(trees) => {
				this._trees = trees;
				if (this._trees.length === 0) return;
				this._createTreeRoutes();
			}
		);
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

		this.observe<ManifestSectionView[]>(
			this._sectionContext.data.pipe(
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
