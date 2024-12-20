import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardContext } from './clipboard.context.js';
import type { UmbClipboardEntryDetailModel } from '../clipboard-entry/index.js';
import { UMB_CLIPBOARD_ENTRY_ENTITY_TYPE } from '../clipboard-entry/entity.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let context: UmbClipboardContext;
	const textClipboardEntry: UmbClipboardEntryDetailModel = {
		entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
		unique: '1',
		type: 'text',
		name: 'Text',
		icon: 'icon-text',
		meta: {},
		value: 'value text',
	};

	const mediaClipboardEntry: UmbClipboardEntryDetailModel = {
		entityType: UMB_CLIPBOARD_ENTRY_ENTITY_TYPE,
		unique: '2',
		type: 'media',
		name: 'Media',
		icon: 'icon-media',
		meta: {},
		value: 'value media',
	};

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		context = new UmbClipboardContext(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a create method', () => {
				expect(context).to.have.property('create').that.is.a('function');
			});

			it('has a read method', () => {
				expect(context).to.have.property('read').that.is.a('function');
			});

			it('has a update method', () => {
				expect(context).to.have.property('update').that.is.a('function');
			});

			it('has a delete method', () => {
				expect(context).to.have.property('delete').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(context).to.have.property('clear').that.is.a('function');
			});
		});
	});

	describe('Create', () => {
		it('throws an error if the entry is not provided', () => {
			// @ts-ignore - Testing invalid input
			expect(() => context.create()).to.throw('Entry is required');
		});

		it('throws an error if the entry does not have a unique property', () => {
			// @ts-ignore - Testing invalid input
			expect(() => context.create({})).to.throw('Entry must have a unique property');
		});

		it('creates an entry in the clipboard', () => {
			context.create(textClipboardEntry);
			expect(context.getEntries()).to.have.lengthOf(1);
			const entry = context.read('1');
			expect(entry).to.deep.equal(textClipboardEntry);
		});
	});

	describe('Read', () => {
		it('throws an error if the entry is not found', () => {
			expect(() => context.read('1')).to.throw('Entry with unique 1 not found');
		});

		it('reads an entry from the clipboard', () => {
			context.create(textClipboardEntry);
			const entry = context.read('1');
			expect(entry).to.deep.equal(textClipboardEntry);
		});
	});

	describe('Update', () => {
		it('throws an error if the entry is not provided', () => {
			// @ts-ignore - Testing invalid input
			expect(() => context.update()).to.throw('Entry is required');
		});

		it('throws an error if the entry does not have a unique property', () => {
			// @ts-ignore - Testing invalid input
			expect(() => context.update({})).to.throw('Entry must have a unique property');
		});

		it('updates an entry in the clipboard', () => {
			context.create(textClipboardEntry);
			const updatedEntry = { ...textClipboardEntry, name: 'Updated Text' };
			context.update(updatedEntry);
			const entry = context.readForProperty('1');
			expect(entry).to.deep.equal(updatedEntry);
		});
	});

	describe('Delete', () => {
		it('deletes an entry from the clipboard', () => {
			context.create(textClipboardEntry);
			context.delete('1');
			expect(context.getEntries).to.have.lengthOf(0);
			expect(() => context.readForProperty('1')).to.throw('Entry with unique 1 not found');
		});
	});

	describe('Clear', () => {
		it('clears all entries from the clipboard', () => {
			context.create(textClipboardEntry);
			context.clear();
			expect(context.getEntries).to.have.lengthOf(0);
		});
	});
});
