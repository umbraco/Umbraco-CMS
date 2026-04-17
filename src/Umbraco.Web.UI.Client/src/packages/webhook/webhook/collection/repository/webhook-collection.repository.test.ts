import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';
import { UmbWebhookCollectionRepository } from './webhook-collection.repository.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbWebhookCollectionRepository', () => {
	let hostElement: UmbTestControllerHostElement;
	let repository: UmbWebhookCollectionRepository;

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		repository = new UmbWebhookCollectionRepository(hostElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a requestCollection method', () => {
				expect(repository).to.have.property('requestCollection').that.is.a('function');
			});
		});
	});

	describe('requestCollection', () => {
		it('returns a collection of webhooks', async () => {
			const { data, error } = await repository.requestCollection({ skip: 0, take: 3 });
			expect(error).to.be.undefined;
			expect(data).to.exist;
			expect(data!.items).to.be.an('array').that.is.not.empty;
			expect(data!.total).to.be.greaterThan(0);
		});

		it('returns items with the correct UmbWebhookDetailModel shape', async () => {
			const { data } = await repository.requestCollection({ skip: 0, take: 1 });
			const item = data!.items[0];
			expect(item).to.have.property('unique').that.is.a('string');
			expect(item).to.have.property('entityType').that.equals(UMB_WEBHOOK_ENTITY_TYPE);
			expect(item).to.have.property('url').that.is.a('string');
			expect(item).to.have.property('enabled').that.is.a('boolean');
			expect(item).to.have.property('events').that.is.an('array');
		});

		it('respects the take filter', async () => {
			const { data } = await repository.requestCollection({ skip: 0, take: 3 });
			expect(data!.items).to.have.lengthOf(3);
		});

		it('respects the skip filter', async () => {
			const { data: firstPage } = await repository.requestCollection({ skip: 0, take: 2 });
			const { data: skipped } = await repository.requestCollection({ skip: 1, take: 2 });
			expect(skipped!.items[0].unique).to.equal(firstPage!.items[1].unique);
		});

		it('returns the same total regardless of pagination', async () => {
			const { data: firstPage } = await repository.requestCollection({ skip: 0, take: 3 });
			const { data: secondPage } = await repository.requestCollection({ skip: 3, take: 3 });
			expect(firstPage!.total).to.equal(secondPage!.total);
		});
	});
});
