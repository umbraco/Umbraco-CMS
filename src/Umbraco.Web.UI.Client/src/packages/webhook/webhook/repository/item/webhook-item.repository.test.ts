import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbWebhookItemRepository } from './webhook-item.repository.js';
import { UmbWebhookItemStore } from './webhook-item.store.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbWebhookItemStore(this);
	}
}

describe('UmbWebhookItemRepository', () => {
	let hostElement: UmbTestControllerHostElement;
	let repository: UmbWebhookItemRepository;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		repository = new UmbWebhookItemRepository(hostElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a requestItems method', () => {
				expect(repository).to.have.property('requestItems').that.is.a('function');
			});

			it('has an items method', () => {
				expect(repository).to.have.property('items').that.is.a('function');
			});
		});
	});

	describe('requestItems', () => {
		it('returns data for a single known unique', async () => {
			const { data } = await repository.requestItems(['webhook-named-id']);
			expect(data).to.have.lengthOf(1);
			expect(data![0].unique).to.equal('webhook-named-id');
			expect(data![0].name).to.equal('Content Publisher');
		});

		it('returns data for multiple uniques', async () => {
			const { data } = await repository.requestItems(['webhook-named-id', 'webhook-disabled-id']);
			expect(data).to.have.lengthOf(2);
			const uniques = data!.map((item) => item.unique);
			expect(uniques).to.include.members(['webhook-named-id', 'webhook-disabled-id']);
		});

		it('returns items with the correct UmbWebhookItemModel shape', async () => {
			const { data } = await repository.requestItems(['webhook-named-id']);
			const item = data![0];
			expect(item).to.have.property('unique').that.is.a('string');
			expect(item).to.have.property('name');
			expect(item).to.have.property('entityType').that.equals(UMB_WEBHOOK_ENTITY_TYPE);
		});

		it('returns an empty array for an unknown unique', async () => {
			const { data } = await repository.requestItems(['non-existing-webhook-id']);
			expect(data).to.be.empty;
		});
	});

	describe('items', () => {
		it('returns an observable that emits the requested items', async () => {
			await repository.requestItems(['webhook-named-id']);
			const observable = await repository.items(['webhook-named-id']);
			expect(observable).to.exist;

			await new Promise<void>((resolve) => {
				observable!.subscribe((items) => {
					if (items.length > 0) {
						expect(items[0].unique).to.equal('webhook-named-id');
						resolve();
					}
				});
			});
		});
	});
});
