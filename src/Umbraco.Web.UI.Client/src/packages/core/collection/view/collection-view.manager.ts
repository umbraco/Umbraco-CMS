import type { UmbCollectionLayoutConfiguration } from '../types.js';
import type { ManifestCollectionView } from './collection-view.extension.js';
import type { UmbCollectionViewElementBase } from './umb-collection-view-element-base.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { PageComponent, UmbRoute } from '@umbraco-cms/backoffice/router';
import type {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';

const CURRENT_VIEW_MEMORY_UNIQUE = 'UmbCollectionCurrentView';

export interface UmbCollectionViewManagerConfig {
	defaultViewAlias?: string;
	manifestFilter?: (manifest: ManifestCollectionView) => boolean;
	viewsOverride?: Array<UmbCollectionLayoutConfiguration>;
}

/**
 * Construction arguments for {@link UmbCollectionViewManager}.
 */
export interface UmbCollectionViewManagerArgs {
	/**
	 * When provided, the view the user left the collection in is remembered and used as the landing view.
	 */
	interactionMemoryManager?: UmbInteractionMemoryManager;
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
	#viewsOverride?: Array<UmbCollectionLayoutConfiguration>;

	#interactionMemoryManager?: UmbInteractionMemoryManager;
	#muteMemoryObservation = false;
	#fallbackViewAlias?: string;

	/**
	 * @param {UmbControllerHost} host - The controller host this manager is bound to.
	 * @param {UmbCollectionViewManagerArgs} [args] - Optional construction arguments.
	 */
	constructor(host: UmbControllerHost, args?: UmbCollectionViewManagerArgs) {
		super(host);
		this.#interactionMemoryManager = args?.interactionMemoryManager;

		if (this.#interactionMemoryManager) {
			this.#observeInteractionMemory();
		}

		// TODO: hack - we need to figure out how to get the "parent path" from the router
		setTimeout(() => {
			const currentUrl = new URL(window.location.href);
			this.#rootPathName.setValue(currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/')));
		}, 100);
	}

	public setConfig(config: UmbCollectionViewManagerConfig) {
		this.#defaultViewAlias = config.defaultViewAlias;
		this.#viewsOverride = config.viewsOverride;
		this.#observeViews(config.manifestFilter);
	}

	// Views
	/**
	 * Sets the current view.
	 * @param {ManifestCollectionView} view The view to set as current.
	 * @memberof UmbCollectionViewManager
	 */
	public setCurrentView(view: ManifestCollectionView) {
		this.#writeToMemory(view);
		this.#currentView.setValue(view);
	}

	/**
	 * Returns the current view.
	 * @returns {ManifestCollectionView} The current view.
	 * @memberof UmbCollectionViewManager
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
			(manifestInitializer) => {
				let manifests = manifestInitializer.map((view) => view.manifest);

				// Reorder and filter to match the `viewsOverride` array order
				if (this.#viewsOverride?.length) {
					manifests = this.#viewsOverride
						.map((override) => manifests.find((m) => m.alias === override.collectionView))
						.filter((m) => m !== undefined);
				}

				this.#views.setValue(manifests);
				this.#createRoutes(manifests);
			},
		);
	}

	#setupViewComponent(component: PageComponent, view: ManifestCollectionView) {
		(component as HTMLElement).setAttribute('data-mark', `collection-view:${view.alias}`);
		(component as UmbCollectionViewElementBase).manifest = view;
		this.setCurrentView(view);
	}

	#createRoutes(views: ManifestCollectionView[] | null) {
		let routes: Array<UmbRoute> = [];

		if (views && views.length > 0) {
			// find the default view from the config. If it doesn't exist, use the first view
			const firstOverrideView = this.#viewsOverride?.length
				? views.find((view) => view.alias === this.#viewsOverride![0].collectionView)
				: null;
			const defaultView = firstOverrideView ?? views.find((view) => view.alias === this.#defaultViewAlias);
			// The remembered view wins over the configured default, as it is the view the user left the collection in.
			const rememberedView = views.find((view) => view.alias === this.#getMemorizedViewAlias());
			const fallbackView = rememberedView ?? defaultView ?? views[0];
			this.#fallbackViewAlias = fallbackView.alias;

			routes = views.map((view) => {
				return {
					path: `${view.meta.pathName}`,
					component: () => createExtensionElement(view),
					setup: (component) => this.#setupViewComponent(component, view),
				};
			});

			if (routes.length > 0) {
				routes.push({
					unique: fallbackView.alias,
					path: '',
					component: () => createExtensionElement(fallbackView),
					setup: (component) => this.#setupViewComponent(component, fallbackView),
				});

				routes.push({
					path: `**`,
					component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
				});
			}
		}

		this.#routes.setValue(routes);
	}

	#getMemorizedViewAlias(): string | undefined {
		return this.#interactionMemoryManager?.getMemory(CURRENT_VIEW_MEMORY_UNIQUE)?.value?.alias;
	}

	#writeToMemory(view: ManifestCollectionView) {
		if (!this.#interactionMemoryManager) return;
		const memory: UmbInteractionMemoryModel = {
			unique: CURRENT_VIEW_MEMORY_UNIQUE,
			value: { alias: view.alias },
		};
		this.#muteMemoryObservation = true;
		this.#interactionMemoryManager.setMemory(memory);
		this.#muteMemoryObservation = false;
	}

	#observeInteractionMemory() {
		this.observe(
			this.#interactionMemoryManager!.memory(CURRENT_VIEW_MEMORY_UNIQUE),
			(memory) => {
				if (this.#muteMemoryObservation) return;
				if (!memory) return;
				const views = this.#views.getValue();
				if (!views.length) return; // extensions not loaded yet; the initializer callback will handle it
				if (memory.value?.alias === this.#fallbackViewAlias) return;
				// Rebuild the routes so the collection lands on the remembered view when no view is requested by the URL.
				this.#createRoutes(views);
			},
			'umbCollectionViewMemoryObserver',
		);
	}
}
