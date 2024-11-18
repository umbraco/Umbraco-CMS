import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbClipboardContext } from './clipboard.context.js';
import type { UmbClipboardEntry } from '../types.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let context: UmbClipboardContext;
	const clipboardTextEntry: UmbClipboardEntry = {
		unique: '1',
		type: 'text',
		name: 'Text',
		icons: ['icon-text'],
		meta: {},
		data: [],
	};

	const clipboardMediaEntry: UmbClipboardEntry = {
		unique: '2',
		type: 'media',
		name: 'Media',
		icons: ['icon-media'],
		meta: {},
		data: [],
	};

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		context = new UmbClipboardContext(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has an entries property', () => {
				expect(context).to.have.property('entries').to.be.an.instanceOf(Observable);
			});

			it('has a hasEntries property', () => {
				expect(context).to.have.property('hasEntries').to.be.an.instanceOf(Observable);
			});
		});

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

			it('has a observeEntriesOf method', () => {
				expect(context).to.have.property('observeEntriesOf').that.is.a('function');
			});

			it('has a observeHasEntriesOf method', () => {
				expect(context).to.have.property('observeHasEntriesOf').that.is.a('function');
			});
		});
	});

	describe('get entries', () => {
		it('returns an empty array if no entries are in the clipboard', () => {
			expect(context.getEntries()).to.have.lengthOf(0);
		});

		it('returns all entries in the clipboard', () => {
			context.create(clipboardTextEntry);
			expect(context.getEntries()).to.have.lengthOf(1);
		});
	});

	describe('set entries', () => {
		it('throws an error if the entries are not provided', () => {
			// @ts-ignore - Testing invalid input
			expect(() => context.setEntries()).to.throw('Entries are required');
		});

		it('sets entries in the clipboard', () => {
			context.setEntries([clipboardTextEntry]);
			expect(context.getEntries()).to.have.lengthOf(1);
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
			context.create(clipboardTextEntry);
			expect(context.getEntries()).to.have.lengthOf(1);
			const entry = context.read('1');
			expect(entry).to.deep.equal(clipboardTextEntry);
		});
	});

	describe('Read', () => {
		it('throws an error if the entry is not found', () => {
			expect(() => context.read('1')).to.throw('Entry with unique 1 not found');
		});

		it('reads an entry from the clipboard', () => {
			context.create(clipboardTextEntry);
			const entry = context.read('1');
			expect(entry).to.deep.equal(clipboardTextEntry);
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
			context.create(clipboardTextEntry);
			const updatedEntry = { ...clipboardTextEntry, name: 'Updated Text' };
			context.update(updatedEntry);
			const entry = context.read('1');
			expect(entry).to.deep.equal(updatedEntry);
		});
	});

	describe('Delete', () => {
		it('deletes an entry from the clipboard', () => {
			context.create(clipboardTextEntry);
			context.delete('1');
			expect(context.getEntries).to.have.lengthOf(0);
			expect(() => context.read('1')).to.throw('Entry with unique 1 not found');
		});
	});

	describe('Clear', () => {
		it('clears all entries from the clipboard', () => {
			context.create(clipboardTextEntry);
			context.clear();
			expect(context.getEntries).to.have.lengthOf(0);
		});
	});

	describe('hasEntries observable', () => {
		it('emits false when there are no entries', (done) => {
			context.hasEntries.subscribe((hasEntries) => {
				expect(hasEntries).to.be.false;
				done();
			});
		});

		it('emits true when there are entries', (done) => {
			context.create(clipboardTextEntry);
			context.hasEntries.subscribe((hasEntries) => {
				expect(hasEntries).to.be.true;
				done();
			});
		});
	});

	describe('observeHasEntriesOf', () => {
		it('emits true if the clipboard has entries of the specified types', (done) => {
			context.create(clipboardTextEntry);

			context.observeHasEntriesOf(['text']).subscribe((hasEntries) => {
				expect(hasEntries).to.be.true;
				done();
			});
		});

		it('emits false when an entry of a different type is created', (done) => {
			context.create(clipboardMediaEntry);
			context.observeHasEntriesOf(['text']).subscribe((hasEntries) => {
				expect(hasEntries).to.be.false;
				done();
			});
		});
	});
});
