import { resetMockHandlers, useMockHandlers } from '../../../../../mocks/index.js';
import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
import { UMB_LINK_PICKER_LINK_REF_SELECTOR } from './constants.js';
import { UmbLinkPickerLinkRefElement } from './link-picker-link-ref.element.js';
import type { UmbLinkPickerMediaRefElement } from './link-picker-media-ref.element.js';
import './link-picker-media-ref.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { MediaItemResponseModel, MediaUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbMediaItemModel, UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';
import { UMB_MEDIA_ITEM_STORE_CONTEXT, UMB_MEDIA_URL_STORE_CONTEXT } from '@umbraco-cms/backoffice/media';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
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

// The item repositories a link ref looks up through wait for their store, so the refs under test need
// a host that provides one. A failing lookup reports itself as a notification, so that context has to
// be reachable too.
@customElement('umb-test-link-picker-link-ref-host')
class UmbTestLinkPickerLinkRefHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbTestMediaItemStore(this);
		new UmbTestMediaUrlStore(this);
		new UmbNotificationContext(this);
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

const mediaUrl = (unique: string, url: string | null): MediaUrlInfoResponseModel => ({
	id: unique,
	urlInfos: url === null ? [] : [{ culture: null, url }],
});

const mediaLink = (unique: string, link?: Partial<UmbLinkPickerLink>): UmbLinkPickerLink => ({
	type: 'media',
	unique,
	name: '',
	url: '',
	...link,
});

const uniquesOf = (request: Request) => new URL(request.url).searchParams.getAll('id');

describe('UmbLinkPickerLinkRefElement', () => {
	let host: UmbTestLinkPickerLinkRefHostElement;
	let itemRequests: Array<Array<string>>;
	let urlRequests: Array<Array<string>>;

	/** Renders a link ref of the given tag inside the host, so its lookups can reach a store. */
	const renderRef = async <T extends UmbLinkPickerLinkRefElement>(tag: string, link: UmbLinkPickerLink) => {
		const element = document.createElement(tag) as T;
		element.link = link;
		host.appendChild(element);
		await element.updateComplete;
		return element;
	};

	const detailOf = (element: UmbLinkPickerLinkRefElement) =>
		element.shadowRoot?.querySelector('uui-ref-node')?.getAttribute('detail');

	beforeEach(async () => {
		itemRequests = [];
		urlRequests = [];
		host = await fixture(html`<umb-test-link-picker-link-ref-host></umb-test-link-picker-link-ref-host>`);
	});

	afterEach(() => {
		resetMockHandlers();
	});

	describe('a link that needs no lookup', () => {
		it('renders the name and URL it carries without asking for anything', async () => {
			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, ({ request }) => {
					itemRequests.push(uniquesOf(request));
					return HttpResponse.json([]);
				}),
			);

			const element = await renderRef<UmbLinkPickerLinkRefElement>('umb-link-picker-link-ref', {
				type: 'external',
				name: 'Umbraco',
				url: 'https://umbraco.com',
			});

			expect(element.displayName).to.equal('Umbraco');
			expect(detailOf(element)).to.equal('https://umbraco.com');
			expect(itemRequests).to.have.lengthOf(0);
		});

		it('carries the attribute every link ref is addressed by', async () => {
			const element = await renderRef<UmbLinkPickerLinkRefElement>('umb-link-picker-link-ref', {
				type: 'external',
				url: 'https://umbraco.com',
			});

			expect(element.matches(UMB_LINK_PICKER_LINK_REF_SELECTOR)).to.be.true;
		});
	});

	describe('a link that points at an item', () => {
		const respondWithEverything = () => {
			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					itemRequests.push(uniques);
					return HttpResponse.json(uniques.map(mediaItem));
				}),
				http.get(MEDIA_URLS_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					urlRequests.push(uniques);
					return HttpResponse.json(uniques.map((unique) => mediaUrl(unique, `/media/${unique}.jpg`)));
				}),
			);
		};

		it('resolves the name and the URL of the item it points at', async () => {
			respondWithEverything();

			const element = await renderRef<UmbLinkPickerMediaRefElement>('umb-link-picker-media-ref', mediaLink('aaaa'));

			await waitUntil(() => element.displayName === 'Name of aaaa', 'the name was resolved');
			await waitUntil(() => detailOf(element) === '/media/aaaa.jpg', 'the URL was resolved');
		});

		it('looks a unique up once, however often the same link is assigned', async () => {
			respondWithEverything();

			const element = await renderRef<UmbLinkPickerMediaRefElement>('umb-link-picker-media-ref', mediaLink('aaaa'));
			await waitUntil(() => element.displayName === 'Name of aaaa', 'the name was resolved');

			// A list re-rendering — on a re-order, say — assigns every link again, and what it assigns is a
			// new object whenever the state it came from rebuilt it.
			element.link = mediaLink('aaaa');
			element.link = mediaLink('aaaa');

			// Re-pointing the link afterwards gives a lookup to wait for. Both endpoints answer in the
			// order they are asked, so anything the assignments above asked for has arrived by the time
			// this one has.
			element.link = mediaLink('bbbb');
			await waitUntil(
				() => itemRequests.flat().includes('bbbb') && urlRequests.flat().includes('bbbb'),
				'the link it was re-pointed at was looked up',
			);

			expect(itemRequests.flat().filter((unique) => unique === 'aaaa')).to.have.lengthOf(1);
			expect(urlRequests.flat().filter((unique) => unique === 'aaaa')).to.have.lengthOf(1);
		});

		it('does not look up the name of a link that carries one', async () => {
			respondWithEverything();

			const element = await renderRef<UmbLinkPickerMediaRefElement>(
				'umb-link-picker-media-ref',
				mediaLink('aaaa', { name: 'Named by the link' }),
			);
			await waitUntil(() => detailOf(element) === '/media/aaaa.jpg', 'the URL was resolved');

			expect(element.displayName).to.equal('Named by the link');
			expect(itemRequests).to.have.lengthOf(0);
		});

		it('does not look up the URL of a link picked for a culture', async () => {
			respondWithEverything();

			const element = await renderRef<UmbLinkPickerMediaRefElement>(
				'umb-link-picker-media-ref',
				mediaLink('aaaa', { culture: 'en-us', url: '/en/carried.jpg' }),
			);
			await waitUntil(() => element.displayName === 'Name of aaaa', 'the name was resolved');

			expect(urlRequests).to.have.lengthOf(0);
			expect(detailOf(element)).to.equal('/en/carried.jpg');
		});

		it('looks up the item it is re-pointed at, and shows nothing of the previous one meanwhile', async () => {
			let releaseSecondLookup: () => void = () => {};
			const secondLookup = new Promise<void>((resolve) => {
				releaseSecondLookup = resolve;
			});

			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, async ({ request }) => {
					const uniques = uniquesOf(request);
					itemRequests.push(uniques);
					if (uniques.includes('bbbb')) await secondLookup;
					return HttpResponse.json(uniques.map(mediaItem));
				}),
				http.get(MEDIA_URLS_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					urlRequests.push(uniques);
					return HttpResponse.json(uniques.map((unique) => mediaUrl(unique, `/media/${unique}.jpg`)));
				}),
			);

			const element = await renderRef<UmbLinkPickerMediaRefElement>('umb-link-picker-media-ref', mediaLink('aaaa'));
			await waitUntil(() => element.displayName === 'Name of aaaa', 'the first name was resolved');

			element.link = mediaLink('bbbb');
			await waitUntil(() => itemRequests.length === 2, 'the second name was requested');

			expect(element.displayName, 'the name of the item before it is not shown for this one').to.equal('');

			releaseSecondLookup();
			await waitUntil(() => element.displayName === 'Name of bbbb', 'the second name was resolved');
		});
	});

	describe('a lookup that comes back without a value', () => {
		it('settles, and is not asked again', async () => {
			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, ({ request }) => {
					itemRequests.push(uniquesOf(request));
					return HttpResponse.json([]);
				}),
				http.get(MEDIA_URLS_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					urlRequests.push(uniques);
					return HttpResponse.json(uniques.map((unique) => mediaUrl(unique, null)));
				}),
			);

			const element = await renderRef<UmbLinkPickerMediaRefElement>('umb-link-picker-media-ref', mediaLink('aaaa'));
			await waitUntil(
				() => itemRequests.flat().includes('aaaa') && urlRequests.flat().includes('aaaa'),
				'both lookups were made',
			);

			element.link = mediaLink('aaaa');
			element.link = mediaLink('aaaa');

			// Re-pointing the link afterwards gives a lookup to wait for, so the assignments above have
			// been answered by the time this one has.
			element.link = mediaLink('bbbb');
			await waitUntil(
				() => itemRequests.flat().includes('bbbb') && urlRequests.flat().includes('bbbb'),
				'the link it was re-pointed at was looked up',
			);

			expect(
				itemRequests.flat().filter((unique) => unique === 'aaaa'),
				'the name is not asked for again',
			).to.have.lengthOf(1);
			expect(
				urlRequests.flat().filter((unique) => unique === 'aaaa'),
				'the URL is not asked for again',
			).to.have.lengthOf(1);
		});
	});

	describe('a lookup that could not be performed', () => {
		it('is retried when the link is assigned again', async () => {
			useMockHandlers(
				http.get(MEDIA_ITEM_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					itemRequests.push(uniques);
					if (itemRequests.length === 1) return new HttpResponse(null, { status: 500 });
					return HttpResponse.json(uniques.map(mediaItem));
				}),
				http.get(MEDIA_URLS_PATH, ({ request }) => {
					const uniques = uniquesOf(request);
					urlRequests.push(uniques);
					if (urlRequests.length === 1) return new HttpResponse(null, { status: 500 });
					return HttpResponse.json(uniques.map((unique) => mediaUrl(unique, `/media/${unique}.jpg`)));
				}),
			);

			const element = await renderRef<UmbLinkPickerMediaRefElement>('umb-link-picker-media-ref', mediaLink('aaaa'));
			await waitUntil(() => itemRequests.length >= 1 && urlRequests.length >= 1, 'both lookups failed');

			element.link = mediaLink('aaaa');

			await waitUntil(() => element.displayName === 'Name of aaaa', 'the name was resolved on the retry');
			await waitUntil(() => detailOf(element) === '/media/aaaa.jpg', 'the URL was resolved on the retry');
		});
	});
});
