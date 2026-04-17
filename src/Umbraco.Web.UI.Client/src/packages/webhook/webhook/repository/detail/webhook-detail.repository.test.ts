import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import type { UmbWebhookDetailModel } from '../../../types.js';
import { UMB_WEBHOOK_ENTITY_TYPE } from '../../../entity.js';
import { UmbWebhookDetailRepository } from './webhook-detail.repository.js';
import { UmbWebhookDetailStore } from './webhook-detail.store.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbWebhookDetailStore(this);
		new UmbNotificationContext(this);
	}
}

describe('UmbWebhookDetailRepository', () => {
	let hostElement: UmbTestControllerHostElement;
	let repository: UmbWebhookDetailRepository;

	const webhookDetailModel: UmbWebhookDetailModel = {
		entityType: UMB_WEBHOOK_ENTITY_TYPE,
		unique: 'test-webhook-unique',
		name: 'Test Webhook',
		description: 'A test webhook',
		url: 'https://example.com/webhook',
		enabled: true,
		headers: { 'X-Test': 'value' },
		events: [{ alias: 'Umbraco.ContentPublish', eventName: 'Content Published', eventType: 'Content' }],
		contentTypes: [],
	};

	before(async () => {
		await useMockSet('kitchenSink');
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		repository = new UmbWebhookDetailRepository(hostElement);
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a createScaffold method', () => {
				expect(repository).to.have.property('createScaffold').that.is.a('function');
			});

			it('has a create method', () => {
				expect(repository).to.have.property('create').that.is.a('function');
			});

			it('has a requestByUnique method', () => {
				expect(repository).to.have.property('requestByUnique').that.is.a('function');
			});

			it('has a save method', () => {
				expect(repository).to.have.property('save').that.is.a('function');
			});

			it('has a delete method', () => {
				expect(repository).to.have.property('delete').that.is.a('function');
			});
		});
	});

	describe('createScaffold', () => {
		it('returns a scaffold with default values', async () => {
			const { data } = await repository.createScaffold();
			expect(data).to.exist;
			expect(data!.entityType).to.equal(UMB_WEBHOOK_ENTITY_TYPE);
			expect(data!.enabled).to.be.true;
			expect(data!.url).to.equal('');
			expect(data!.events).to.deep.equal([]);
			expect(data!.contentTypes).to.deep.equal([]);
		});

		it('merges preset values into the scaffold', async () => {
			const { data } = await repository.createScaffold({ name: 'Preset Webhook', url: 'https://preset.example.com' });
			expect(data!.name).to.equal('Preset Webhook');
			expect(data!.url).to.equal('https://preset.example.com');
		});
	});

	describe('requestByUnique', () => {
		it('returns data for a known unique', async () => {
			const { data, error } = await repository.requestByUnique('webhook-named-id');
			expect(error).to.be.undefined;
			expect(data).to.exist;
			expect(data!.unique).to.equal('webhook-named-id');
			expect(data!.name).to.equal('Content Publisher');
			expect(data!.entityType).to.equal(UMB_WEBHOOK_ENTITY_TYPE);
		});

		it('returns an error for an unknown unique', async () => {
			const { data, error } = await repository.requestByUnique('non-existing-webhook-id');
			expect(data).to.be.undefined;
			expect(error).to.exist;
		});
	});

	describe('create', () => {
		it('creates a new webhook and returns it', async () => {
			const { data, error } = await repository.create(webhookDetailModel);
			expect(error).to.be.undefined;
			expect(data).to.exist;
			expect(data!.name).to.equal(webhookDetailModel.name);
			expect(data!.url).to.equal(webhookDetailModel.url);
			expect(data!.enabled).to.equal(webhookDetailModel.enabled);
			expect(data!.entityType).to.equal(UMB_WEBHOOK_ENTITY_TYPE);
		});
	});

	describe('save', () => {
		it('updates an existing webhook', async () => {
			const { data: created } = await repository.create(webhookDetailModel);
			const updated = { ...created!, name: 'Updated Webhook', url: 'https://updated.example.com' };
			const { data, error } = await repository.save(updated);
			expect(error).to.be.undefined;
			expect(data!.name).to.equal('Updated Webhook');
			expect(data!.url).to.equal('https://updated.example.com');
		});
	});

	describe('delete', () => {
		it('deletes an existing webhook', async () => {
			const { data: created } = await repository.create(webhookDetailModel);
			const { error: deleteError } = await repository.delete(created!.unique);
			expect(deleteError).to.be.undefined;

			const { data, error: fetchError } = await repository.requestByUnique(created!.unique);
			expect(data).to.be.undefined;
			expect(fetchError).to.exist;
		});
	});
});
