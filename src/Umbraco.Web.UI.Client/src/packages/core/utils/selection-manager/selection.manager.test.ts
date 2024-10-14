import { UmbSelectionManager } from './selection.manager.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbSelectionManager', () => {
	let manager: UmbSelectionManager;

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbSelectionManager(hostElement);
		manager.setSelectable(true);
		manager.setMultiple(true);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a selectable property', () => {
				expect(manager).to.have.property('selectable').to.be.an.instanceOf(Observable);
			});

			it('has a selection property', () => {
				expect(manager).to.have.property('selection').to.be.an.instanceOf(Observable);
			});

			it('has a multiple property', () => {
				expect(manager).to.have.property('multiple').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a getSelectable method', () => {
				expect(manager).to.have.property('getSelectable').that.is.a('function');
			});

			it('has a setSelectable method', () => {
				expect(manager).to.have.property('setSelectable').that.is.a('function');
			});

			it('has a getSelection method', () => {
				expect(manager).to.have.property('getSelection').that.is.a('function');
			});

			it('has a setSelection method', () => {
				expect(manager).to.have.property('setSelection').that.is.a('function');
			});

			it('has a getMultiple method', () => {
				expect(manager).to.have.property('getMultiple').that.is.a('function');
			});

			it('has a setMultiple method', () => {
				expect(manager).to.have.property('setMultiple').that.is.a('function');
			});

			it('has a toggleSelect method', () => {
				expect(manager).to.have.property('toggleSelect').that.is.a('function');
			});

			it('has a select method', () => {
				expect(manager).to.have.property('select').that.is.a('function');
			});

			it('has a deselect method', () => {
				expect(manager).to.have.property('deselect').that.is.a('function');
			});

			it('has a isSelected method', () => {
				expect(manager).to.have.property('isSelected').that.is.a('function');
			});

			it('has a clearSelection method', () => {
				expect(manager).to.have.property('clearSelection').that.is.a('function');
			});

			it('has a setAllowLimitation method', () => {
				expect(manager).to.have.property('setAllowLimitation').that.is.a('function');
			});
		});
	});

	describe('Selectable', () => {
		it('sets and gets the selectable value', () => {
			manager.setSelectable(false);
			expect(manager.getSelectable()).to.equal(false);
		});

		it('updates the observable', (done) => {
			manager.setSelectable(false);

			manager.selectable
				.subscribe((value) => {
					expect(value).to.equal(false);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Selection', () => {
		it('sets and gets the selection value', () => {
			manager.setSelection(['1', '2']);
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});

		it('updates the observable', (done) => {
			manager.setSelection(['1', '2']);

			manager.selection
				.subscribe((value) => {
					expect(value).to.deep.equal(['1', '2']);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Multiple', () => {
		it('sets and gets the multiple value', () => {
			manager.setMultiple(true);
			expect(manager.getMultiple()).to.equal(true);
		});

		it('updates the observable', (done) => {
			manager.setMultiple(true);

			manager.multiple
				.subscribe((value) => {
					expect(value).to.equal(true);
					done();
				})
				.unsubscribe();
		});
	});

	describe('Select', () => {
		it('selects an item', () => {
			manager.select('3');
			expect(manager.getSelection()).to.deep.equal(['3']);
		});

		it('does nothing if the item is already selected', () => {
			manager.setSelection(['1', '2']);
			manager.select('2');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});

		it('does nothing if selection isnt supported', () => {
			manager.setSelectable(false);
			manager.select('3');
			expect(manager.getSelection()).to.deep.equal([]);
		});

		it('can not select an item if it does not pass the allow function', () => {
			manager.setAllowLimitation((item) => item !== '2');
			expect(() => manager.select('2')).to.throw();
			expect(manager.getSelection()).to.deep.equal([]);

			manager.select('1');
			expect(manager.getSelection()).to.deep.equal(['1']);
		});
	});

	describe('Deselect', () => {
		it('deselects an item', () => {
			manager.setSelection(['1', '2', '3']);
			manager.deselect('2');
			expect(manager.getSelection()).to.deep.equal(['1', '3']);
		});

		it('does nothing if the item isnt selected', () => {
			manager.setSelection(['1', '2']);
			manager.deselect('3');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});

		it('does nothing if selection isnt supported', () => {
			manager.setSelection(['1', '2']);
			manager.setSelectable(false);
			manager.deselect('2');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});
	});

	describe('Toggle select', () => {
		it('toggle selects an item', () => {
			manager.toggleSelect('1');
			manager.toggleSelect('2');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
			manager.toggleSelect('1');
			expect(manager.getSelection()).to.deep.equal(['2']);
		});

		it('does nothing if selection isnt supported', () => {
			manager.setSelectable(false);
			manager.toggleSelect('1');
			manager.toggleSelect('2');
			expect(manager.getSelection()).to.deep.equal([]);
		});
	});

	describe('Is selected', () => {
		it('returns true if the item is selected', () => {
			manager.setSelection(['1', '2']);
			expect(manager.isSelected('1')).to.equal(true);
		});

		it('returns false if the item isnt selected', () => {
			manager.setSelection(['1', '2']);
			expect(manager.isSelected('3')).to.equal(false);
		});
	});

	describe('Clear selection', () => {
		it('clears the selection', () => {
			manager.setSelection(['1', '2']);
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
			manager.clearSelection();
			expect(manager.getSelection()).to.deep.equal([]);
		});

		it('does nothing if selection isnt supported', () => {
			manager.setSelection(['1', '2']);
			manager.setSelectable(false);
			manager.clearSelection();
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});
	});

	describe('Multi selection', () => {
		it('selects multiple items', () => {
			manager.select('1');
			manager.select('2');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});

		it('cant do the selection if the selection contains an item that is not allowed', () => {
			manager.setAllowLimitation((item) => item !== '2');
			expect(() => manager.setSelection(['1', '2'])).to.throw();
			expect(manager.getSelection()).to.deep.equal([]);
		});

		it('deselects multiple items', () => {
			manager.setSelection(['1', '2', '3']);
			manager.deselect('1');
			manager.deselect('2');
			expect(manager.getSelection()).to.deep.equal(['3']);
		});

		it('toggles multiple items', () => {
			manager.toggleSelect('1');
			manager.toggleSelect('2');
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
			manager.toggleSelect('1');
			expect(manager.getSelection()).to.deep.equal(['2']);
		});
	});

	describe('Single selection', () => {
		it('selects a single item', () => {
			manager.setMultiple(false);
			manager.select('1');
			manager.select('2');
			expect(manager.getSelection()).to.deep.equal(['2']);
		});

		it('keeps the first item if multiple is disabled mid selection', () => {
			manager.select('1');
			manager.select('2');
			manager.setMultiple(false);
			expect(manager.getSelection()).to.deep.equal(['1']);
		});
	});
});
