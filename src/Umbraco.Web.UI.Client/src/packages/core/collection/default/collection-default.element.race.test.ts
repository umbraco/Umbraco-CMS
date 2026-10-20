import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import type { UmbCollectionDefaultElement } from './collection-default.element.js';
import type { ManifestCollectionView } from '../view/types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import './collection-default.element.js';

// Collection views are created from their manifest on demand, so two view changes in quick succession leave two
// element creations in flight at once. A view backed by a lazy import resolves slower than one backed by a statically
// imported constructor, so the creations can settle in the opposite order to the changes that started them.

@customElement('umb-test-race-slow-collection-view')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceSlowCollectionViewElement extends HTMLElement {}

@customElement('umb-test-race-fast-collection-view')
class UmbTestRaceFastCollectionViewElement extends HTMLElement {}

class UmbFakeCollectionViewManager {
	#currentView = new UmbObjectState<ManifestCollectionView | undefined>(undefined);
	currentView = this.#currentView.asObservable();

	setCurrentView(view: ManifestCollectionView | undefined) {
		this.#currentView.setValue(view);
	}

	getCurrentView() {
		return this.#currentView.getValue();
	}
}

class UmbFakeCollectionContext {
	#host: UmbControllerHostElement;

	view = new UmbFakeCollectionViewManager();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	#loading = new UmbBooleanState(false);
	loading = this.#loading.asObservable();

	#totalItems = new UmbNumberState(0);
	totalItems = this.#totalItems.asObservable();

	getEmptyLabel = () => 'empty';
	loadCollection = () => {};

	setLoading(value: boolean) {
		this.#loading.setValue(value);
	}
}

@customElement('umb-test-race-collection-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceCollectionHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbCollectionDefaultElement view creation', () => {
	let element: UmbCollectionDefaultElement;
	let context: UmbFakeCollectionContext;
	let releaseSlowView: () => void;
	let slowView: ManifestCollectionView;
	let fastView: ManifestCollectionView;

	beforeEach(async () => {
		let release!: () => void;
		const gate = new Promise<void>((resolve) => (release = resolve));
		releaseSlowView = release;

		slowView = {
			type: 'collectionView',
			alias: 'Umb.Test.Race.CollectionView.Slow',
			name: 'Slow Collection View',
			element: async () => {
				await gate;
				return { element: UmbTestRaceSlowCollectionViewElement };
			},
			meta: { label: 'Slow', icon: 'icon-list', pathName: 'slow' },
		} as unknown as ManifestCollectionView;

		fastView = {
			type: 'collectionView',
			alias: 'Umb.Test.Race.CollectionView.Fast',
			name: 'Fast Collection View',
			element: UmbTestRaceFastCollectionViewElement,
			meta: { label: 'Fast', icon: 'icon-grid', pathName: 'fast' },
		} as unknown as ManifestCollectionView;

		const host = await fixture<UmbTestRaceCollectionHostElement>(
			html`<umb-test-race-collection-host>
				<umb-collection-default></umb-collection-default>
			</umb-test-race-collection-host>`,
		);

		context = new UmbFakeCollectionContext(host);
		host.provideContext(UMB_COLLECTION_CONTEXT, context as never);

		element = host.querySelector('umb-collection-default') as UmbCollectionDefaultElement;

		// The empty state is withheld until the first load has completed, and the view is only rendered alongside it.
		context.setLoading(true);
		await aTimeout(0);
		context.setLoading(false);
		await aTimeout(0);
	});

	const queryRenderedView = () => element.shadowRoot?.querySelector('#view')?.firstElementChild;

	it('renders the view it was asked for', async () => {
		context.view.setCurrentView(slowView);
		releaseSlowView();
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView()?.localName).to.equal('umb-test-race-slow-collection-view');
	});

	it('renders the current view when an earlier view is created after it', async () => {
		context.view.setCurrentView(slowView);
		await aTimeout(0);

		context.view.setCurrentView(fastView);
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView()?.localName).to.equal('umb-test-race-fast-collection-view');

		releaseSlowView();
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView()?.localName).to.equal('umb-test-race-fast-collection-view');
	});
});
