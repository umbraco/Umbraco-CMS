import { expect } from '@open-wc/testing';
import { UmbClipboardLocalStorageManager } from './clipboard-local-storage.manager.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from './clipboard-entry/entity.js';
import type { UmbClipboardEntryDetailModel } from './clipboard-entry/index.js';

type UmbTestClipboardEntryType = 'test1' | 'test2' | 'test3' | 'test4';
interface UmbTestClipboardEntryDetailModel
	extends UmbClipboardEntryDetailModel<UmbTestClipboardEntryType, object, string> {}

describe('UmbClipboardLocalStorageManager', () => {
	let manager: UmbClipboardLocalStorageManager;
	const clipboardEntries: Array<UmbTestClipboardEntryDetailModel> = [
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			value: 'test1',
			icon: 'icon1',
			meta: {},
			name: 'Test1',
			type: 'test1',
			unique: '1',
			createDate: null,
			updateDate: null,
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			value: 'test2',
			icon: 'icon2',
			meta: {},
			name: 'Test2',
			type: 'test2',
			unique: '2',
			createDate: null,
			updateDate: null,
		},
		{
			entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
			value: 'test3',
			icon: 'icon3',
			meta: {},
			name: 'Test3',
			type: 'test3',
			unique: '3',
			createDate: null,
			updateDate: null,
		},
	];

	beforeEach(() => {
		localStorage.clear();
		manager = new UmbClipboardLocalStorageManager();
		manager.setEntries(clipboardEntries);
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
		it('returns all entries from local storage', () => {
			const { entries, total } = manager.getEntries();
			expect(entries).to.deep.equal(clipboardEntries);
			expect(total).to.equal(clipboardEntries.length);
		});
	});

	describe('getEntry', () => {
		it('returns a single entry from local storage', () => {
			const { entry } = manager.getEntry('2');
			expect(entry).to.deep.equal(clipboardEntries[1]);
		});
	});

	describe('setEntries', () => {
		it('sets entries in local storage', () => {
			const newEntry: UmbClipboardEntryDetailModel = {
				entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
				value: 'test4',
				icon: 'icon4',
				meta: {},
				name: 'Test4',
				type: 'test4',
				unique: '4',
				createDate: null,
				updateDate: null,
			};
			manager.setEntries([...clipboardEntries, newEntry]);
			const { entries, total } = manager.getEntries();
			expect(entries).to.deep.equal([...clipboardEntries, newEntry]);
			expect(total).to.equal(clipboardEntries.length + 1);
		});
	});
});
