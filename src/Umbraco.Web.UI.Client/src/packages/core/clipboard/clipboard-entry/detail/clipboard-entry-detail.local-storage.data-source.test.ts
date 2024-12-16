import { expect } from '@open-wc/testing';
import { UmbClipboardEntryDetailLocalStorageDataSource } from './clipboard-entry-detail.local-storage.data-source.js';
import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../entity.js';

describe('UmbClipboardEntryDetailLocalStorageDataSource', () => {
	let dataSource: UmbClipboardEntryDetailLocalStorageDataSource;
	const clipboardEntry: UmbClipboardEntryDetailModel = {
		entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
		value: 'test',
		icon: 'icon',
		meta: {},
		name: 'Test',
		type: 'test',
		unique: '123',
	};

	beforeEach(() => {
		localStorage.clear();
		dataSource = new UmbClipboardEntryDetailLocalStorageDataSource();
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a create method', () => {
				expect(dataSource).to.have.property('create').that.is.a('function');
			});

			it('has a read method', () => {
				expect(dataSource).to.have.property('read').that.is.a('function');
			});

			it('has a update method', () => {
				expect(dataSource).to.have.property('update').that.is.a('function');
			});

			it('has a delete method', () => {
				expect(dataSource).to.have.property('delete').that.is.a('function');
			});
		});
	});

	describe('Create', () => {
		it('creates a new entry', async () => {
			const response = await dataSource.create(clipboardEntry);
			expect(response.data).to.deep.equal(clipboardEntry);
		});

		it('returns an error if entry is missing', async () => {
			// @ts-expect-error - Testing error case
			const response = await dataSource.create();
			expect(response.error).to.be.an.instanceOf(Error);
		});
	});

	describe('Read', () => {
		it('reads an entry', async () => {
			await dataSource.create(clipboardEntry);
			const response = await dataSource.read(clipboardEntry.unique);
			expect(response.data).to.deep.equal(clipboardEntry);
		});

		it('returns an error if unique is missing', async () => {
			// @ts-expect-error - Testing error case
			const response = await dataSource.read();
			expect(response.error).to.be.an.instanceOf(Error);
		});

		it('returns an error if entry is not found', async () => {
			const response = await dataSource.read('123');
			expect(response.error).to.be.an.instanceOf(Error);
		});
	});

	describe('Update', () => {
		it('updates an entry', async () => {
			await dataSource.create(clipboardEntry);
			const updatedEntry = { ...clipboardEntry, data: ['updated'] };
			const response = await dataSource.update(updatedEntry);
			expect(response.data).to.deep.equal(updatedEntry);
		});

		it('returns an error if entry is missing', async () => {
			// @ts-expect-error - Testing error case
			const response = await dataSource.update();
			expect(response.error).to.be.an.instanceOf(Error);
		});

		it('returns an error if entry is not found', async () => {
			const response = await dataSource.update(clipboardEntry);
			expect(response.error).to.be.an.instanceOf(Error);
		});
	});

	describe('Delete', () => {
		it('deletes an entry', async () => {
			await dataSource.create(clipboardEntry);
			await dataSource.delete(clipboardEntry.unique);
			const response = await dataSource.read(clipboardEntry.unique);
			expect(response.data).to.be.undefined;
		});

		it('returns an error if unique is missing', async () => {
			// @ts-expect-error - Testing error case
			const response = await dataSource.delete();
			expect(response.error).to.be.an.instanceOf(Error);
		});

		it('returns an error if entry is not found', async () => {
			const response = await dataSource.delete('not-existing');
			expect(response.error).to.be.an.instanceOf(Error);
		});
	});
});
