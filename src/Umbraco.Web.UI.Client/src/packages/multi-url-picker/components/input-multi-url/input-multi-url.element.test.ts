import { resetMockHandlers, useMockHandlers } from '../../../../../mocks/index.js';
import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
import { UmbInputMultiUrlElement } from './input-multi-url.element.js';
import { expect, fixture, html, oneEvent, waitUntil } from '@open-wc/testing';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { MediaItemResponseModel, MediaUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UMB_INTERACTION_MEMORY_SCOPE_CONTEXT,
	UmbInteractionMemoriesChangeEvent,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbMediaItemModel, UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';
import { UMB_MEDIA_ITEM_STORE_CONTEXT, UMB_MEDIA_URL_STORE_CONTEXT } from '@umbraco-cms/backoffice/media';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

const MEDIA_ITEM_PATH = umbracoPath('/item/media');
const MEDIA_URLS_PATH = umbracoPath('/media/urls');

class UmbTestMediaItemStore extends UmbItemStoreBase<UmbMediaItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_STORE_CONTEXT);
	}
}

class UmbTestMediaUrlStore extends UmbItemStoreBase<UmbMediaUrlModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_URL_STORE_CONTEXT);
	}
}

// The link refs this input renders look their items up through the item repositories, which wait for
// their store, so the input under test needs a host that provides one.
@customElement('umb-test-input-multi-url-host')
class UmbTestInputMultiUrlHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbTestMediaItemStore(this);
		new UmbTestMediaUrlStore(this);
	}
}

const mediaItem = (unique: string): MediaItemResponseModel => ({
	id: unique,
	isTrashed: false,
	hasChildren: false,
	parent: null,
	mediaType: { id: 'media-type-unique', icon: 'icon-picture', collection: null },
	variants: [{ culture: null, name: `Name of ${unique}` }],
	flags: [],
});

const mediaUrl = (unique: string): MediaUrlInfoResponseModel => ({
	id: unique,
	urlInfos: [{ culture: null, url: `/media/${unique}.jpg` }],
});

const mediaLink = (unique: string): UmbLinkPickerLink => ({ type: 'media', unique, name: '', url: '' });

const uniquesOf = (request: Request) => new URL(request.url).searchParams.getAll('id');

describe('UmbInputMultiUrlElement', () => {
	let element: UmbInputMultiUrlElement;

	beforeEach(async () => {
		const host = await fixture<UmbTestInputMultiUrlHostElement>(
			html`<umb-test-input-multi-url-host>
				<umb-input-multi-url></umb-input-multi-url>
			</umb-test-input-multi-url-host>`,
		);
		element = host.querySelector<UmbInputMultiUrlElement>('umb-input-multi-url')!;
		await element.updateComplete;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMultiUrlElement);
	});

	// The link picker modal is opened through a modal route, so it is never a descendant of this
	// element — context is the only channel it can read the memories from.
	describe('interaction memory', () => {
		const memory = {
			unique: 'UmbLinkPickerModal',
			memories: [{ unique: 'location', value: { unique: 'folder-1' } }],
		};

		it('provides itself as the interaction-memory scope for its modals', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			expect(scope).to.not.equal(undefined);
		});

		it('makes memories set on the property reachable through the scope', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			element.interactionMemories = [memory];
			expect(scope!.getMemory('UmbLinkPickerModal')).to.deep.equal(memory);
		});

		it('drops memories that are no longer present when the property is set again', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			element.interactionMemories = [memory];
			element.interactionMemories = [];
			expect(scope!.getMemory('UmbLinkPickerModal')).to.equal(undefined);
		});

		it('dispatches interaction-memories-change and exposes the memory when the scope is written to', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			const listener = oneEvent(element, UmbInteractionMemoriesChangeEvent.TYPE);
			scope!.setMemory(memory);
			await listener;
			expect(element.interactionMemories).to.deep.equal([memory]);
		});

		it('does not dispatch interaction-memories-change for memories it was just handed', async () => {
			await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT);
			let dispatched = false;
			element.addEventListener(UmbInteractionMemoriesChangeEvent.TYPE, () => (dispatched = true));
			element.interactionMemories = [memory];
			await new Promise((resolve) => setTimeout(resolve, 10));
			expect(dispatched).to.equal(false);
		});
	});

	// What a link displays is looked up from the item it points at, so the list has to be able to
	// render and re-order again without asking for the same information twice.
	describe('resolving the links it renders', () => {
		let itemRequests: Array<Array<string>>;
		let urlRequests: Array<Array<string>>;

		const requestCount = () => itemRequests.length + urlRequests.length;

		const renderedNames = () =>
			Array.from(element.shadowRoot?.querySelectorAll('umb-link-picker-media-ref') ?? []).map((ref) => ref.displayName);

		const setUrls = async (urls: Array<UmbLinkPickerLink>) => {
			element.urls = urls;
			await element.updateComplete;
		};

		const withThreeResolvedLinks = async () => {
			await setUrls([mediaLink('aaaa'), mediaLink('bbbb'), mediaLink('cccc')]);
			await waitUntil(
				() => renderedNames().join() === 'Name of aaaa,Name of bbbb,Name of cccc',
				'every link was resolved',
			);
		};

		beforeEach(() => {
			itemRequests = [];
			urlRequests = [];

			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					itemRequests.push(uniques);
					return HttpResponse.json(uniques.map(mediaItem));
				}),
				http.get(MEDIA_URLS_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					urlRequests.push(uniques);
					return HttpResponse.json(uniques.map(mediaUrl));
				}),
			);
		});

		afterEach(() => {
			resetMockHandlers();
		});

		it('resolves the name and the URL of every link it is given', async () => {
			await withThreeResolvedLinks();

			expect(itemRequests.flat()).to.have.members(['aaaa', 'bbbb', 'cccc']);
			expect(urlRequests.flat()).to.have.members(['aaaa', 'bbbb', 'cccc']);
		});

		it('does not resolve the links again when they are only re-ordered', async () => {
			await withThreeResolvedLinks();
			const before = requestCount();

			// The value the sorter hands back on a re-order: the same links, in a different order.
			await setUrls([mediaLink('cccc'), mediaLink('aaaa'), mediaLink('bbbb')]);

			expect(requestCount()).to.equal(before);
			expect(renderedNames().join()).to.equal('Name of cccc,Name of aaaa,Name of bbbb');
		});

		it('resolves only the link that was added to the list', async () => {
			await withThreeResolvedLinks();
			itemRequests = [];
			urlRequests = [];

			await setUrls([mediaLink('aaaa'), mediaLink('bbbb'), mediaLink('cccc'), mediaLink('dddd')]);
			await waitUntil(() => renderedNames().includes('Name of dddd'), 'the added link was resolved');

			expect(itemRequests.flat()).to.eql(['dddd']);
			expect(urlRequests.flat()).to.eql(['dddd']);
		});

		// A consumer that addresses a link by the name it is displayed under — the acceptance test helpers
		// do, to remove or edit one — reaches its actions from there, so the element carrying the name has
		// to be the element the actions sit in.
		it('renders the actions of a link within the element carrying its name', async () => {
			await withThreeResolvedLinks();

			const named = element.shadowRoot!.querySelectorAll('[name="Name of bbbb"]');

			expect(named, 'exactly one element is identified by the name').to.have.lengthOf(1);
			// The label is the localization key here, as nothing localizes it in a test run.
			expect(named[0].querySelector('uui-action-bar uui-button')?.getAttribute('label')).to.equal('general_remove');
		});

		it('does not resolve the remaining links when one is removed', async () => {
			await withThreeResolvedLinks();
			const before = requestCount();

			await setUrls([mediaLink('aaaa'), mediaLink('cccc')]);

			expect(requestCount()).to.equal(before);
			expect(renderedNames().join()).to.equal('Name of aaaa,Name of cccc');
		});
	});
});
