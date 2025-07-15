import { expect } from '@open-wc/testing';
import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../entity.js';
import UmbClipboardEntryDetailRepository from './clipboard-entry-detail.repository.js';
import UmbClipboardEntryDetailStore from './clipboard-entry-detail.store.js';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbCurrentUserStore(this);
		new UmbClipboardEntryDetailStore(this);
		new UmbNotificationContext(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbClipboardEntryDetailRepository', () => {
	let hostElement: UmbTestControllerHostElement;
	let repository: UmbClipboardEntryDetailRepository;
	const detailData: UmbClipboardEntryDetailModel = {
		entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
		values: [{ type: 'test', value: 'test' }],
		icon: 'icon',
		meta: {},
		name: 'Test',
		unique: '123',
		createDate: null,
		updateDate: null,
	};

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		repository = new UmbClipboardEntryDetailRepository(hostElement);
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		localStorage.clear();
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
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

	describe('Create', () => {
		it('creates a new entry', async () => {
			const response = await repository.create(detailData);
			expect(response.data).to.deep.equal({
				...detailData,
				// TODO: this is not testing anything. We can't use the response data to check the createDate and updateDate
				createDate: response.data?.createDate,
				updateDate: response.data?.updateDate,
			});
		});
	});

	describe('requestByUnique', () => {
		it('requests an entry', async () => {
			await repository.create(detailData);
			const response = await repository.requestByUnique(detailData.unique);
			expect(response.data).to.deep.equal({
				...detailData,
				// TODO: this is not testing anything. We can't use the response data to check the createDate and updateDate
				createDate: response.data?.createDate,
				updateDate: response.data?.updateDate,
			});
		});
	});

	describe('Update', () => {
		it('updates an entry', async () => {
			await repository.create(detailData);
			const updatedEntry = { ...detailData, value: 'updated' };
			const response = await repository.save(updatedEntry);
			expect(response.data).to.deep.equal({
				...updatedEntry,
				// TODO: this is not testing anything. We can't use the response data to check the createDate and updateDate
				createDate: response.data?.createDate,
				updateDate: response.data?.updateDate,
			});
		});
	});

	describe('Delete', () => {
		it('deletes an entry', async () => {
			// create an entry
			await repository.create(detailData);

			// delete it
			await repository.delete(detailData.unique);
			const response = await repository.requestByUnique(detailData.unique);
			expect(response.data).to.be.undefined;
		});
	});
});
