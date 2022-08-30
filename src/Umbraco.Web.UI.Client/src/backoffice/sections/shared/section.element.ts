import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription, map, switchMap, EMPTY, of } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';
import type { ManifestTree, ManifestEditor } from '../../../core/models';

import '../shared/section-trees.element.ts';
import { UmbEditorElement } from '../../editors/shared/editor-entity/editor-entity.element';
import { UmbEntityStore } from '../../../core/stores/entity.store';

@customElement('umb-section')
export class UmbSectionElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
			}
		`,
	];

	// TODO: make this code reusable across sections
	@state()
	private _routes: Array<any> = [];

	@state()
	private _trees?: Array<ManifestTree>;

	private _editors?: Array<ManifestEditor>;
	private _editorsSubscription?: Subscription;

	private _entityStore?: UmbEntityStore;

	private _sectionContext?: UmbSectionContext;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionsRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionsRegistry;
			this._useTrees();
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._useTrees();
		});

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			this._useTrees();
		});
	}

	private _useTrees() {
		if (!this._sectionContext || !this._extensionRegistry || !this._entityStore) return;

		this._treesSubscription?.unsubscribe();

		this._treesSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionRegistry
							?.extensionsOfType('tree')
							.pipe(
								map((trees) =>
									trees
										.filter((tree) => tree.meta.sections.includes(section.alias))
										.sort((a, b) => b.meta.weight - a.meta.weight)
								)
							) ?? of([])
					);
				})
			)
			.subscribe((trees) => {
				this._trees = trees;
				this._createRoutes();
			});
	}

	private _createRoutes() {
		const treeRoutes =
			this._trees?.map(() => {
				return {
					path: `:entityType/:key`,
					component: () => import('../../editors/shared/editor-entity/editor-entity.element'),
					setup: (component: UmbEditorElement, info: any) => {
						component.entityKey = info.match.params.key;
						component.entityType = info.match.params.entityType;
					},
				};
			}) ?? [];

		this._routes = [
			{
				path: 'dashboard',
				component: () => import('../shared/section-dashboards.element'),
			},
			...treeRoutes,
			{
				path: '**',
				redirectTo: 'dashboard',
			},
		];
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._editorsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-section-layout>
				<umb-section-sidebar>
					<umb-section-trees></umb-section-trees>
				</umb-section-sidebar>
				<umb-section-main>
					<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
				</umb-section-main>
			</umb-section-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section': UmbSectionElement;
	}
}
