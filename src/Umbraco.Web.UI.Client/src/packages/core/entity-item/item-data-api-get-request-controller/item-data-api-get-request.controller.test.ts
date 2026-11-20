import { UmbItemDataApiGetRequestController } from './item-data-api-get-request.controller.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

@customElement('test-item-data-api-get-request-controller-host')
class UmbTestItemDataApiGetRequestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbNotificationContext(this);
	}
}

describe('UmbItemDataApiGetRequestController', () => {
	let hostElement: UmbTestItemDataApiGetRequestControllerHostElement;

	const createUniques = (count: number) => Array.from({ length: count }, (_, index) => `item-${index}`);

	beforeEach(() => {
		hostElement = new UmbTestItemDataApiGetRequestControllerHostElement();
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('returns the items of every chunk when all chunks succeed', async () => {
		const uniques = createUniques(45);
		const controller = new UmbItemDataApiGetRequestController(hostElement, {
			api: async (args) => ({ data: args.uniques.map((unique) => ({ unique })) }),
			uniques,
		});

		const { data, error } = await controller.request();

		expect(error).to.be.undefined;
		expect(data?.map((item) => item.unique)).to.deep.equal(uniques);
	});

	it('reports an error and omits the failed chunk when a chunk fails', async () => {
		const uniques = createUniques(45);
		const controller = new UmbItemDataApiGetRequestController(hostElement, {
			api: async (args) => {
				if (args.uniques.includes('item-0')) throw new Error('Chunk failed');
				return { data: args.uniques.map((unique) => ({ unique })) };
			},
			uniques,
		});

		const { data, error } = await controller.request();

		expect(error).to.not.be.undefined;
		expect(data).to.not.include(undefined);
		expect(data?.map((item) => item.unique)).to.deep.equal(uniques.slice(40));
	});
});
