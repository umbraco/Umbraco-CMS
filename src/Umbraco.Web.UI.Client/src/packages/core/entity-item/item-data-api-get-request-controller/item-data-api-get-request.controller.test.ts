import { UmbItemDataApiGetRequestController } from './item-data-api-get-request.controller.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';

@customElement('umb-test-item-data-api-get-request-host')
class UmbTestItemDataApiGetRequestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface UmbTestItem {
	id: string;
}

describe('UmbItemDataApiGetRequestController', () => {
	let host: UmbTestItemDataApiGetRequestHostElement;

	// More than the batch size of 40, so the request is split into chunks.
	const chunkedUniques = Array.from({ length: 45 }, (_, index) => `item-${index}`);

	beforeEach(() => {
		host = new UmbTestItemDataApiGetRequestHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('returns all items when every chunk succeeds', async () => {
		const controller = new UmbItemDataApiGetRequestController<{ data: Array<UmbTestItem> }>(host, {
			api: async (args) => ({ data: args.uniques.map((id) => ({ id })) }),
			uniques: chunkedUniques,
			disableNotifications: true,
		});

		const { data, error } = await controller.request();

		expect(error).to.be.undefined;
		expect(data).to.have.lengthOf(45);
	});

	it('returns the items from the chunks that succeeded when one chunk fails', async () => {
		// A failing chunk resolves with an error rather than rejecting, so treating only rejections as failures leaves
		// the caller with an array padded with undefined and no error to explain it.
		const controller = new UmbItemDataApiGetRequestController<{ data: Array<UmbTestItem> }>(host, {
			api: async (args) => {
				if (args.uniques.includes('item-40')) {
					throw { name: 'ApiError', message: 'Request too long' };
				}
				return { data: args.uniques.map((id) => ({ id })) };
			},
			uniques: chunkedUniques,
			disableNotifications: true,
		});

		const { data, error } = await controller.request();

		expect(error).to.exist;
		expect(data).to.have.lengthOf(40);
		expect((data as Array<UmbTestItem>).every((item) => item !== undefined)).to.be.true;
	});

	it('returns the error the failing chunk produced', async () => {
		// The chunk error carries the status and problem details a caller needs, so it must not be flattened into a
		// bare message.
		const controller = new UmbItemDataApiGetRequestController<{ data: Array<UmbTestItem> }>(host, {
			api: async (args) => {
				if (args.uniques.includes('item-40')) {
					throw { name: 'ApiError', message: 'Request too long' };
				}
				return { data: args.uniques.map((id) => ({ id })) };
			},
			uniques: chunkedUniques,
			disableNotifications: true,
		});

		const { error } = await controller.request();

		expect(error).to.be.instanceOf(UmbApiError);
		expect(error?.message).to.not.equal('[object Object]');
	});

	it('returns an error when a single unchunked request fails', async () => {
		const controller = new UmbItemDataApiGetRequestController<{ data: Array<UmbTestItem> }>(host, {
			api: async () => {
				throw { name: 'ApiError', message: 'Request failed' };
			},
			uniques: ['item-1', 'item-2'],
			disableNotifications: true,
		});

		const { data, error } = await controller.request();

		expect(error).to.exist;
		expect(data).to.be.undefined;
	});
});
