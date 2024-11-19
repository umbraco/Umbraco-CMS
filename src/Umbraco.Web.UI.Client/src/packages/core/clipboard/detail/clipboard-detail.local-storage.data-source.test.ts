import { expect, oneEvent } from '@open-wc/testing';
import type { UmbClipboardEntry } from '../types.js';
import { UmbClipboardDetailLocalStorageDataSource } from './clipboard-detail.local-storage.data-source.js';

describe('UmbClipboardLocalStorageDataSource', () => {
	let dataSource: UmbClipboardDetailLocalStorageDataSource;
	const clipboardEntry: UmbClipboardEntry = {
		data: ['test'],
		icons: ['icon'],
		meta: {},
		name: 'Test',
		type: 'test',
		unique: '123',
	};

	beforeEach(() => {
		localStorage.clear();
		dataSource = new UmbClipboardDetailLocalStorageDataSource();
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
	});

	describe('Read', () => {
		it('reads an entry', async () => {
			await dataSource.create(clipboardEntry);
			const response = await dataSource.read(clipboardEntry.unique);
			expect(response.data).to.deep.equal(clipboardEntry);
		});
	});

	describe('Update', () => {
		it('updates an entry', async () => {
			await dataSource.create(clipboardEntry);
			const updatedEntry = { ...clipboardEntry, data: ['updated'] };
			const response = await dataSource.update(updatedEntry);
			expect(response.data).to.deep.equal(updatedEntry);
		});
	});

	describe('Delete', () => {
		it('deletes an entry', async () => {
			await dataSource.create(clipboardEntry);
			await dataSource.delete(clipboardEntry.unique);
			const response = await dataSource.read(clipboardEntry.unique);
			expect(response.data).to.be.undefined;
		});
	});
});
