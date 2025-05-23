import type { ManifestCollectionView } from './extensions/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

export interface UmbCollectionViewManagerConfig {
	defaultViewAlias?: string;
	manifestFilter?: (manifest: ManifestCollectionView) => boolean;
}

export class UmbCollectionViewManager extends UmbControllerBase {
	#views = new UmbArrayState<ManifestCollectionView>([], (x) => x.alias);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	#routes = new UmbArrayState<UmbRoute>([], (x) => x.path);
	public readonly routes = this.#routes.asObservable();

	#rootPathName = new UmbStringState('');
	public readonly rootPathName = this.#rootPathName.asObservable();

	#defaultViewAlias?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: hack - we need to figure out how to get the "parent path" from the router
		setTimeout(() => {
			const currentUrl = new URL(window.location.href);
			this.#rootPathName.setValue(currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/')));
		}, 100);
	}

	public setConfig(config: UmbCollectionViewManagerConfig) {
		this.#defaultViewAlias = config.defaultViewAlias;
		this.#observeViews(config.manifestFilter);
	}

	// Views
	/**
	 * Sets the current view.
	 * @param {ManifestCollectionView} view
	 * @memberof UmbCollectionContext
	 */
	public setCurrentView(view: ManifestCollectionView) {
		this.#currentView.setValue(view);
	}

	/**
	 * Returns the current view.
	 * @returns {ManifestCollectionView}
	 * @memberof UmbCollectionContext
	 */
	public getCurrentView() {
		return this.#currentView.getValue();
	}

	#observeViews(filter?: (manifest: ManifestCollectionView) => boolean) {
		return new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'collectionView',
			filter ?? null,
			(views) => {
				const manifests = views.map((view) => view.manifest);
				this.#views.setValue(manifests);
				this.#createRoutes(manifests);
			},
		);
	}

	#createRoutes(views: ManifestCollectionView[] | null) {
		let routes: Array<UmbRoute> = [];

		if (views && views.length > 0) {
			// find the default view from the config. If it doesn't exist, use the first view
			const defaultView = views.find((view) => view.alias === this.#defaultViewAlias);
			const fallbackView = defaultView ?? views[0];

			routes = views.map((view) => {
				return {
					path: `${view.meta.pathName}`,
					component: () => createExtensionElement(view),
					setup: () => {
						this.setCurrentView(view);
					},
				};
			});

			if (routes.length > 0) {
				routes.push({
					unique: fallbackView.alias,
					path: '',
					component: () => createExtensionElement(fallbackView),
					setup: () => {
						this.setCurrentView(fallbackView);
					},
				});
			}

			routes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		this.#routes.setValue(routes);
	}
}
