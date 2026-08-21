import type { UmbCollectionLayoutConfiguration } from '../types.js';
import type { ManifestCollectionView } from './collection-view.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
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
	#rootPathName = new UmbStringState('');

	#defaultViewAlias?: string;
	#viewsOverride?: Array<UmbCollectionLayoutConfiguration>;

	#interactionMemoryManager?: UmbInteractionMemoryManager;

	/**
	 * A route per view.
	 * @returns {Observable<Array<UmbRoute>>} An always empty observable.
	 * @deprecated Deprecated since v18. The views of a collection are no longer routed, so this observable is always
	 * empty. Observe `currentView` and switch view with `setCurrentView` instead. Scheduled for removal in Umbraco 20.
	 */
	public get routes() {
		new UmbDeprecation({
			removeInVersion: '20.0.0',
			deprecated: 'UmbCollectionViewManager.routes',
			solution: 'The views of a collection are no longer routed. Observe `currentView` instead.',
		}).warn();
		return this.#routes.asObservable();
	}

	/**
	 * The path the view routes were resolved against.
	 * @returns {Observable<string>} An always empty observable.
	 * @deprecated Deprecated since v18. The views of a collection are no longer routed, so this observable is always
	 * empty. Scheduled for removal in Umbraco 20.
	 */
	public get rootPathName() {
		new UmbDeprecation({
			removeInVersion: '20.0.0',
			deprecated: 'UmbCollectionViewManager.rootPathName',
			solution: 'The views of a collection are no longer routed, so there is no root path to resolve them against.',
		}).warn();
		return this.#rootPathName.asObservable();
	}

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
				this.#setLandingView(manifests);
			},
		);
	}

	/**
	 * Lands on the view the user left the collection in, falling back to the configured default and then the first
	 * available view. The view currently shown is kept when it is still available, so views appearing or disappearing
	 * does not move the user.
	 * @param {Array<ManifestCollectionView>} views - The available views.
	 */
	#setLandingView(views: Array<ManifestCollectionView>) {
		if (!views.length) return;

		const currentViewAlias = this.getCurrentView()?.alias;
		if (currentViewAlias && views.some((view) => view.alias === currentViewAlias)) return;

		// find the default view from the config. If it doesn't exist, use the first view
		const firstOverrideView = this.#viewsOverride?.length
			? views.find((view) => view.alias === this.#viewsOverride![0].collectionView)
			: undefined;
		const defaultView = firstOverrideView ?? views.find((view) => view.alias === this.#defaultViewAlias);
		// The remembered view wins over the configured default, as it is the view the user left the collection in.
		const rememberedView = views.find((view) => view.alias === this.#getMemorizedViewAlias());

		this.setCurrentView(rememberedView ?? defaultView ?? views[0]);
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
		this.#interactionMemoryManager.setMemory(memory);
	}

	#observeInteractionMemory() {
		this.observe(
			this.#interactionMemoryManager!.memory(CURRENT_VIEW_MEMORY_UNIQUE),
			(memory) => {
				if (!memory) return;
				const view = this.#views.getValue().find((x) => x.alias === memory.value?.alias);
				// The views have not loaded yet; the extensions initializer lands on the remembered view itself.
				if (!view) return;
				this.#currentView.setValue(view);
			},
			'umbCollectionViewMemoryObserver',
		);
	}
}
