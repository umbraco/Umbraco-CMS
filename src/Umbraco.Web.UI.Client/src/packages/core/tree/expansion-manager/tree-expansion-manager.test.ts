import { UmbTreeExpansionManager } from './tree-expansion-manager.js';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTreeExpansionManager', () => {
	let manager: UmbTreeExpansionManager;
	const item = { entityType: 'test', unique: '123' };
	const item2 = { entityType: 'test', unique: '456' };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbTreeExpansionManager(hostElement);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has an expansion property', () => {
				expect(manager).to.have.property('expansion').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has an isExpanded method', () => {
				expect(manager).to.have.property('isExpanded').that.is.a('function');
			});

			it('has a setExpansion method', () => {
				expect(manager).to.have.property('setExpansion').that.is.a('function');
			});

			it('has a getExpansion method', () => {
				expect(manager).to.have.property('getExpansion').that.is.a('function');
			});

			it('has a expandItem method', () => {
				expect(manager).to.have.property('expandItem').that.is.a('function');
			});

			it('has a collapseItem method', () => {
				expect(manager).to.have.property('collapseItem').that.is.a('function');
			});

			it('has a collapseAll method', () => {
				expect(manager).to.have.property('collapseAll').that.is.a('function');
			});
		});
	});

	describe('isExpanded', () => {
		it('checks if an item is expanded', (done) => {
			manager.setExpansion([item]);
			const isExpanded = manager.isExpanded(item);
			expect(isExpanded).to.be.an.instanceOf(Observable);
			manager.isExpanded(item).subscribe((value) => {
				expect(value).to.be.true;
				done();
			});
		});
	});

	describe('setExpansion', () => {
		it('sets the expansion state', () => {
			const expansion = [item];
			manager.setExpansion(expansion);
			expect(manager.getExpansion()).to.deep.equal(expansion);
		});
	});

	describe('getExpansion', () => {
		it('gets the expansion state', () => {
			const expansion = [item];
			manager.setExpansion(expansion);
			expect(manager.getExpansion()).to.deep.equal(expansion);
		});
	});

	describe('expandItem', () => {
		it('expands an item', async () => {
			await manager.expandItem(item);
			expect(manager.getExpansion()).to.deep.equal([item]);
		});
	});

	describe('collapseItem', () => {
		it('collapses an item', async () => {
			await manager.expandItem(item);
			expect(manager.getExpansion()).to.deep.equal([item]);
			manager.collapseItem(item);
			expect(manager.getExpansion()).to.deep.equal([]);
		});
	});

	describe('collapseAll', () => {
		it('collapses all items', () => {
			manager.setExpansion([item, item2]);
			expect(manager.getExpansion()).to.deep.equal([item, item2]);
			manager.collapseAll();
			expect(manager.getExpansion()).to.deep.equal([]);
		});
	});

	describe('expandTo', () => {
		const nodeA = { entityType: 'test', unique: 'a' };
		const nodeB = { entityType: 'test', unique: 'b' };
		const nodeC = { entityType: 'test', unique: 'c' };
		const nodeD = { entityType: 'test', unique: 'd' };

		const makeRepository = (ancestors: Array<{ entityType: string; unique: string }>, error?: Error) =>
			({
				requestTreeItemAncestors: async () => (error ? { data: undefined, error } : { data: ancestors }),
			}) as unknown as UmbTreeRepository;

		it('has an expandTo method', () => {
			expect(manager).to.have.property('expandTo').that.is.a('function');
		});

		it('builds a linked chain from ancestors + target', async () => {
			await manager.expandTo(nodeD, { repository: makeRepository([nodeA, nodeB, nodeC]) });
			expect(manager.getExpansion()).to.deep.equal([
				{ entityType: 'test', unique: 'a', target: { entityType: 'test', unique: 'b' } },
				{ entityType: 'test', unique: 'b', target: { entityType: 'test', unique: 'c' } },
				{ entityType: 'test', unique: 'c', target: { entityType: 'test', unique: 'd' } },
				{ entityType: 'test', unique: 'd' },
			]);
		});

		it('clips the chain to startNode when provided', async () => {
			await manager.expandTo(nodeD, {
				repository: makeRepository([nodeA, nodeB, nodeC]),
				startNode: nodeB,
			});
			expect(manager.getExpansion()).to.deep.equal([
				{ entityType: 'test', unique: 'b', target: { entityType: 'test', unique: 'c' } },
				{ entityType: 'test', unique: 'c', target: { entityType: 'test', unique: 'd' } },
				{ entityType: 'test', unique: 'd' },
			]);
		});

		it('sets a single-entry chain when the target has no ancestors', async () => {
			await manager.expandTo(nodeA, { repository: makeRepository([]) });
			expect(manager.getExpansion()).to.deep.equal([{ entityType: 'test', unique: 'a' }]);
		});

		it('leaves expansion unchanged on repository error', async () => {
			manager.setExpansion([nodeA]);
			await manager.expandTo(nodeD, { repository: makeRepository([], new Error('network error')) });
			expect(manager.getExpansion()).to.deep.equal([nodeA]);
		});
	});
});
