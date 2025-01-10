import { expect } from '@open-wc/testing';
import {
	UmbClipboardEntryDetailRepository,
	UmbClipboardEntryDetailStore,
	type UmbClipboardEntryDetailModel,
} from '../../clipboard-entry/index.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../../clipboard-entry/entity.js';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbClipboardCollectionRepository } from './clipboard-collection.repository.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbClipboardEntryDetailStore(this);
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbClipboardLocalStorageDataSource', () => {
	let hostElement: UmbTestControllerHostElement;
	let detailRepository: UmbClipboardEntryDetailRepository;
	let collectionRepository: UmbClipboardCollectionRepository;
	const clipboardEntries: Array<UmbClipboardEntryDetailModel> = [
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			values: [{ type: 'test', value: 'test1' }],
			icon: 'icon1',
			meta: {},
			name: 'Test1',
			unique: '1',
			createDate: null,
			updateDate: null,
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			values: [{ type: 'test', value: 'test2' }],
			icon: 'icon2',
			meta: {},
			name: 'Test2',
			unique: '2',
			createDate: null,
			updateDate: null,
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			values: [{ type: 'test', value: 'test3' }],
			icon: 'icon3',
			meta: {},
			name: 'Test3',
			unique: '3',
			createDate: null,
			updateDate: null,
		},
	];

	beforeEach(async () => {
		localStorage.clear();
		hostElement = new UmbTestControllerHostElement();
		detailRepository = new UmbClipboardEntryDetailRepository(hostElement);
		collectionRepository = new UmbClipboardCollectionRepository(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
		await hostElement.init();
		await detailRepository.create(clipboardEntries[0]);
		await detailRepository.create(clipboardEntries[1]);
		await detailRepository.create(clipboardEntries[2]);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a getCollection method', () => {
				expect(collectionRepository).to.have.property('getCollection').that.is.a('function');
			});
		});
	});

	describe('getCollection', () => {
		it('should return all clipboard entries', async () => {
			const result = await collectionRepository.requestCollection({});

			expect(result.data.items).to.have.lengthOf(clipboardEntries.length);
			expect(result.data.items).to.deep.equal(clipboardEntries);
		});
	});
});
