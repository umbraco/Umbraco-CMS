import { UmbCollectionSelectionManager } from './collection-selection.manager.js';
import type { UmbCollectionItemModel, UmbCollectionSelectionConfiguration } from '../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-collection-selection-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const createMockItem = (unique: string): UmbCollectionItemModel => ({
	unique,
	entityType: 'test-entity',
});

describe('UmbCollectionSelectionManager', () => {
	let manager: UmbCollectionSelectionManager;
	let hostElement: UmbTestControllerHostElement;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		manager = new UmbCollectionSelectionManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a selectableFilter property', () => {
				expect(manager).to.have.property('selectableFilter').that.is.a('function');
			});
		});

		describe('methods', () => {
			it('has a setConfig method', () => {
				expect(manager).to.have.property('setConfig').that.is.a('function');
			});

			it('has a getConfig method', () => {
				expect(manager).to.have.property('getConfig').that.is.a('function');
			});
		});
	});

	describe('Configuration', () => {
		it('returns undefined when no config is set', () => {
			expect(manager.getConfig()).to.be.undefined;
		});

		it('sets and gets the configuration', () => {
			const config: UmbCollectionSelectionConfiguration = {
				selectable: true,
				multiple: true,
			};
			manager.setConfig(config);
			expect(manager.getConfig()).to.equal(config);
		});

		it('sets selectable from config', () => {
			manager.setConfig({ selectable: true });
			expect(manager.getSelectable()).to.equal(true);
		});

		it('sets multiple from config', () => {
			manager.setConfig({ selectable: true, multiple: true });
			expect(manager.getMultiple()).to.equal(true);
		});

		it('sets initial selection from config', () => {
			manager.setConfig({ selectable: true, multiple: true, selection: ['1', '2'] });
			expect(manager.getSelection()).to.deep.equal(['1', '2']);
		});

		it('sets selectableFilter from config', () => {
			const customFilter = (item: UmbCollectionItemModel) => item.unique !== 'blocked';
			manager.setConfig({ selectable: true, selectableFilter: customFilter });
			expect(manager.selectableFilter(createMockItem('allowed'))).to.equal(true);
			expect(manager.selectableFilter(createMockItem('blocked'))).to.equal(false);
		});

		it('handles undefined config gracefully', () => {
			manager.setConfig(undefined);
			expect(manager.getConfig()).to.be.undefined;
			expect(manager.getSelectable()).to.equal(false);
			expect(manager.getMultiple()).to.equal(false);
			expect(manager.getSelection()).to.deep.equal([]);
		});

		it('defaults selectable to false when not provided', () => {
			manager.setConfig({});
			expect(manager.getSelectable()).to.equal(false);
		});

		it('defaults multiple to false when not provided', () => {
			manager.setConfig({});
			expect(manager.getMultiple()).to.equal(false);
		});
	});

	describe('selectableFilter', () => {
		it('default filter returns true for any item', () => {
			expect(manager.selectableFilter(createMockItem('any-item'))).to.equal(true);
		});

		it('custom filter is applied from config', () => {
			const customFilter = (item: UmbCollectionItemModel) => item.unique.startsWith('allowed-');
			manager.setConfig({ selectable: true, selectableFilter: customFilter });

			expect(manager.selectableFilter(createMockItem('allowed-1'))).to.equal(true);
			expect(manager.selectableFilter(createMockItem('allowed-2'))).to.equal(true);
			expect(manager.selectableFilter(createMockItem('blocked-1'))).to.equal(false);
		});
	});

	describe('select', () => {
		beforeEach(() => {
			manager.setConfig({ selectable: true, multiple: true });
		});

		it('selects the item', () => {
			manager.select('1');
			expect(manager.getSelection()).to.deep.equal(['1']);
		});
	});

	describe('deselect', () => {
		beforeEach(() => {
			manager.setConfig({ selectable: true, multiple: true, selection: ['1', '2'] });
		});

		it('deselects the item', () => {
			manager.deselect('1');
			expect(manager.getSelection()).to.deep.equal(['2']);
		});
	});

	describe('clearSelection', () => {
		beforeEach(() => {
			manager.setConfig({ selectable: true, multiple: true, selection: ['1', '2'] });
		});

		it('clears all selected items', () => {
			manager.clearSelection();
			expect(manager.getSelection()).to.deep.equal([]);
		});
	});
});
