import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbCollectionViewManager extends UmbBaseController {
	#views = new UmbArrayState<ManifestCollectionView>([], (x) => x.alias);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	#rootPathname = new UmbStringState('');
	public readonly rootPathname = this.#rootPathname.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.#observeViews();

		// TODO: hack - we need to figure out how to get the "parent path" from the router
		setTimeout(() => {
			const currentUrl = new URL(window.location.href);
			this.#rootPathname.next(currentUrl.pathname.substring(0, currentUrl.pathname.lastIndexOf('/')));
		}, 100);
	}

	// Views
	/**
	 * Sets the current view.
	 * @param {ManifestCollectionView} view
	 * @memberof UmbCollectionContext
	 */
	public setCurrentView(view: ManifestCollectionView) {
		this.#currentView.next(view);
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
			this.#views.next(views.map((view) => view.manifest));
			this.#initCurrentView();
		});
	}

	#initCurrentView() {
		// if we already have a current view, don't set it again
		if (this.#currentView.getValue()) return;

		const currentUrl = new URL(window.location.href);
		const lastPathSegment = currentUrl.pathname.split('/').pop();
		const views = this.#views.getValue();
		const viewMatch = views.find((view) => view.meta.pathName === lastPathSegment);

		/* TODO: Find a way to figure out which layout it starts with and set _currentLayout to that instead of [0]. eg. '/table'
			For document, media and members this will come as part of a data type configuration, but in other cases "users" we should find another way.
			This should only happen if the current layout is not set in the URL.
		*/
		const currentView = viewMatch || views[0];
		this.setCurrentView(currentView);
	}
}
