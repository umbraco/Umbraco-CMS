import { expect } from '@open-wc/testing';
import { UmbClipboardLocalStorageManager } from './clipboard-local-storage.manager.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from './clipboard-entry/entity.js';
import type { UmbClipboardEntryDetailModel } from './clipboard-entry/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

interface UmbTestClipboardEntryDetailModel extends UmbClipboardEntryDetailModel<object> {}

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}
}

describe('UmbClipboardLocalStorageManager', () => {
	let hostElement: UmbTestControllerHostElement;
	let manager: UmbClipboardLocalStorageManager;
	const clipboardEntries: Array<UmbTestClipboardEntryDetailModel> = [
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
		hostElement = new UmbTestControllerHostElement();
		manager = new UmbClipboardLocalStorageManager(hostElement);
		document.body.appendChild(hostElement);
		await hostElement.init();
		await manager.setEntries(clipboardEntries);
	});

	afterEach(() => {
		localStorage.clear();
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a getEntries method', () => {
				expect(manager).to.have.property('getEntries').that.is.a('function');
			});

			it('has a getEntry method', () => {
				expect(manager).to.have.property('getEntry').that.is.a('function');
			});

			it('has a setEntries method', () => {
				expect(manager).to.have.property('setEntries').that.is.a('function');
			});
		});
	});

	describe('getEntries', () => {
		it('returns all entries from local storage', async () => {
			const { entries, total } = await manager.getEntries();
			expect(entries).to.deep.equal(clipboardEntries);
			expect(total).to.equal(clipboardEntries.length);
		});
	});

	describe('getEntry', () => {
		it('returns a single entry from local storage', async () => {
			const entry = await manager.getEntry('2');
			expect(entry).to.deep.equal(clipboardEntries[1]);
		});
	});

	describe('setEntries', () => {
		it('sets entries in local storage', async () => {
			const newEntry: UmbClipboardEntryDetailModel = {
				entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
				values: [{ type: 'test', value: 'test4' }],
				icon: 'icon4',
				meta: {},
				name: 'Test4',
				unique: '4',
				createDate: null,
				updateDate: null,
			};
			await manager.setEntries([...clipboardEntries, newEntry]);
			const { entries, total } = await manager.getEntries();
			expect(entries).to.deep.equal([...clipboardEntries, newEntry]);
			expect(total).to.equal(clipboardEntries.length + 1);
		});
	});
});
