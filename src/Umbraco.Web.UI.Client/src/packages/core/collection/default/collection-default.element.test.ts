import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import type { UmbCollectionDefaultElement } from './collection-default.element.js';
import type { ManifestCollectionView } from '../view/types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbFilterManagerContext } from '@umbraco-cms/backoffice/filter';
import { UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import './collection-default.element.js';

@customElement('umb-test-collection-default-view')
class UmbTestCollectionDefaultViewElement extends HTMLElement {}

const view = {
	type: 'collectionView',
	alias: 'Umb.Test.CollectionDefault.View',
	name: 'Test Collection View',
	element: UmbTestCollectionDefaultViewElement,
	meta: { label: 'Test', icon: 'icon-list', pathName: 'test' },
} as unknown as ManifestCollectionView;

class UmbFakeCollectionContext {
	#host: UmbControllerHostElement;

	view = {
		currentView: new UmbObjectState<ManifestCollectionView | undefined>(view).asObservable(),
		getCurrentView: () => view,
	};

	pagination = {
		currentPage: new UmbNumberState(1).asObservable(),
		pageSize: new UmbNumberState(50).asObservable(),
	};

	filtering: UmbFilterManagerContext;

	#loading = new UmbBooleanState(false);
	loading = this.#loading.asObservable();

	#totalItems = new UmbNumberState(0);
	totalItems = this.#totalItems.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.filtering = new UmbFilterManagerContext(host);
	}

	getHostElement() {
		return this.#host;
	}

	getEmptyLabel = () => 'empty';
	loadCollection = () => {};

	setLoading(value: boolean) {
		this.#loading.setValue(value);
	}

	setTotalItems(value: number) {
		this.#totalItems.setValue(value);
	}
}

@customElement('umb-test-collection-default-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestCollectionDefaultHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbCollectionDefaultElement collection info', () => {
	let element: UmbCollectionDefaultElement;
	let context: UmbFakeCollectionContext;

	const collectionInfo = () => element.shadowRoot?.querySelector('#collection-info');

	const settle = async () => {
		await aTimeout(0);
		await element.updateComplete;
	};

	beforeEach(async () => {
		const host = await fixture<UmbTestCollectionDefaultHostElement>(
			html`<umb-test-collection-default-host>
				<umb-collection-default></umb-collection-default>
			</umb-test-collection-default-host>`,
		);

		context = new UmbFakeCollectionContext(host);
		host.provideContext(UMB_COLLECTION_CONTEXT, context as never);

		element = host.querySelector('umb-collection-default') as UmbCollectionDefaultElement;

		context.setLoading(true);
		await aTimeout(0);
		context.setLoading(false);
		await settle();
	});

	it('keeps the active filters when the filtered collection has no items', async () => {
		context.filtering.setFilterValues('Umb.Test.Filter', [{ unique: 'a', value: 'a' }]);
		await settle();

		expect(collectionInfo()?.querySelector('umb-collection-active-filters')).to.exist;
		expect(collectionInfo()?.textContent).not.to.contain('Showing');
	});

	it('shows the item count alongside the active filters when there are items', async () => {
		context.setTotalItems(3);
		await settle();

		expect(collectionInfo()?.textContent).to.contain('Showing 1-3 of 3');
	});

	it('renders no collection info without items or active filters', async () => {
		expect(collectionInfo()).to.be.null;
	});
});
