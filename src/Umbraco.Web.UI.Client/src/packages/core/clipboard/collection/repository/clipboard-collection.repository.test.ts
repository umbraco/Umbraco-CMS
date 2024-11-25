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

@customElement('test-my-app-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbClipboardEntryDetailStore(this);
		new UmbNotificationContext(this);
	}
}

describe('UmbClipboardLocalStorageDataSource', () => {
	let hostElement: UmbTestControllerHostElement;
	let detailRepository: UmbClipboardEntryDetailRepository;
	let collectionRepository: UmbClipboardCollectionRepository;
	const clipboardEntries: Array<UmbClipboardEntryDetailModel> = [
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			data: ['test1'],
			icons: ['icon1'],
			meta: {},
			name: 'Test1',
			type: 'test1',
			unique: '1',
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			data: ['test2'],
			icons: ['icon2'],
			meta: {},
			name: 'Test2',
			type: 'test2',
			unique: '2',
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			data: ['test3'],
			icons: ['icon3'],
			meta: {},
			name: 'Test3',
			type: 'test3',
			unique: '3',
		},
	];

	beforeEach(async () => {
		localStorage.clear();
		hostElement = new UmbTestControllerHostElement();
		detailRepository = new UmbClipboardEntryDetailRepository(hostElement);
		collectionRepository = new UmbClipboardCollectionRepository(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
		const createPromises = Promise.all(
			clipboardEntries.map((clipboardEntry) => detailRepository.create(clipboardEntry)),
		);

		await createPromises;
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
