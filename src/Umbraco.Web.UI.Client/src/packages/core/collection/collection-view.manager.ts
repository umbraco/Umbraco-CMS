import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionsManifestInitializer, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestCollectionView} from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

export interface UmbCollectionViewManagerConfig {
	defaultViewAlias?: string;
}

export class UmbCollectionViewManager extends UmbBaseController {
	#views = new UmbArrayState<ManifestCollectionView>([], (x) => x.alias);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	#routes = new UmbArrayState<UmbRoute>([], (x) => x.path);
	public readonly routes = this.#routes.asObservable();

	#rootPathname = new UmbStringState('');
	public readonly rootPathname = this.#rootPathname.asObservable();

	#defaultViewAlias?: string;

	constructor(host: UmbControllerHost, config: UmbCollectionViewManagerConfig) {
		super(host);

		this.#defaultViewAlias = config.defaultViewAlias;
		this.#observeViews();

		// TODO: hack - we need to figure out how to get the "parent path" from the router
		setTimeout(() => {
			const currentUrl = new URL(window.location.href);
			this.#rootPathname.setValue(currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/')));
		}, 100);
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
	 * @return {ManifestCollectionView}
	 * @memberof UmbCollectionContext
	 */
	public getCurrentView() {
		return this.#currentView.getValue();
	}

	#observeViews() {
		return new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'collectionView', null, (views) => {
			const manifests = views.map((view) => view.manifest);
			this.#views.setValue(manifests);
			this.#createRoutes(manifests);
		});
	}

	#createRoutes(views: ManifestCollectionView[] | null) {
		let routes: Array<UmbRoute> = [];

		if (views) {
			// find the default view from the config. If it doesn't exist, use the first view
			const defaultView = views.find((view) => view.alias === this.#defaultViewAlias);
			const fallbackView = defaultView?.meta.pathName || views[0].meta.pathName;

			routes = views.map((view) => {
				return {
					path: `${view.meta.pathName}`,
					component: () => createExtensionElement(view),
					setup: () => {
						this.setCurrentView(view);
					},
				};
			});

			routes.push({
				path: '',
				redirectTo: fallbackView,
			});
		}

		this.#routes.setValue(routes);
	}
}
