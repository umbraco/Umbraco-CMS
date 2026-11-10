import { resetMockHandlers, useMockHandlers } from '../../../../../../mocks/index.js';
import { manifests as itemRepositoryManifests } from '../../repository/item/manifests.js';
import { UmbMemberGroupItemStore } from '../../repository/item/member-group-item.store.js';
import { UmbInputMemberGroupElement } from './input-member-group.element.js';
import { expect, fixture, html, nextFrame, waitUntil } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

// Provides the item store the input's repository waits for.
@customElement('umb-test-input-member-group-host')
class UmbTestInputMemberGroupHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbMemberGroupItemStore(this);
	}
}

const memberGroupItem = (unique: string): MemberGroupItemResponseModel => ({
	id: unique,
	name: `Name of ${unique}`,
	flags: [],
});

describe('UmbInputMemberGroupElement', () => {
	let element: UmbInputMemberGroupElement;

	const itemRefs = () => Array.from(element.shadowRoot?.querySelectorAll('umb-entity-item-ref') ?? []);

	const renderWith = async (selection: Array<string>, max?: number) => {
		if (max !== undefined) {
			element.max = max;
		}
		element.selection = selection;
		await waitUntil(() => itemRefs().length === selection.length, 'every member group was rendered');
		// The sorter sets up its items on the next animation frame.
		await nextFrame();
	};

	const isDraggableAfterMouseDown = (itemRef: Element) => {
		itemRef.dispatchEvent(new MouseEvent('mousedown', { button: 0, bubbles: true, composed: true }));
		return (itemRef as HTMLElement).draggable;
	};

	before(() => {
		umbExtensionsRegistry.registerMany(itemRepositoryManifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(itemRepositoryManifests.map((manifest) => manifest.alias));
	});

	beforeEach(async () => {
		useMockHandlers(
			http.get(umbracoPath('/item/member-group'), ({ request }) =>
				HttpResponse.json(new URL(request.url).searchParams.getAll('id').map(memberGroupItem)),
			),
		);

		const host = await fixture<UmbTestInputMemberGroupHostElement>(
			html`<umb-test-input-member-group-host>
				<umb-input-member-group></umb-input-member-group>
			</umb-test-input-member-group-host>`,
		);
		element = host.querySelector<UmbInputMemberGroupElement>('umb-input-member-group')!;
		await element.updateComplete;
	});

	afterEach(() => {
		resetMockHandlers();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMemberGroupElement);
	});

	it('lets items be dragged when it can hold more than one item', async () => {
		await renderWith(['group-a', 'group-b']);

		expect(isDraggableAfterMouseDown(itemRefs()[0])).to.be.true;
	});

	it('does not let items be dragged when it can hold at most one item', async () => {
		await renderWith(['group-a'], 1);

		expect(isDraggableAfterMouseDown(itemRefs()[0])).to.be.false;
	});

	it('does not let items be dragged when it is readonly', async () => {
		element.readonly = true;
		await renderWith(['group-a', 'group-b']);

		expect(isDraggableAfterMouseDown(itemRefs()[0])).to.be.false;
	});
});
