import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbInteractionMemoryManager', () => {
	let manager: UmbInteractionMemoryManager;
	const nestedMemory1 = { unique: 'nestedMemory1', value: 'Nested Memory 1' };
	const nestedMemory2 = { unique: 'nestedMemory2', value: 'Nested Memory 2' };
	const memory1 = { unique: '1', value: 'Memory 1' };
	const memory2 = { unique: '2', value: 'Memory 2', memories: [nestedMemory1, nestedMemory2] };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbInteractionMemoryManager(hostElement);
		manager.setMemory(memory1);
		manager.setMemory(memory2);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a memories property', () => {
				expect(manager).to.have.property('memories').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a memory method', () => {
				expect(manager).to.have.property('memory').that.is.a('function');
			});

			it('has a getMemory method', () => {
				expect(manager).to.have.property('getMemory').that.is.a('function');
			});

			it('has a setMemory method', () => {
				expect(manager).to.have.property('setMemory').that.is.a('function');
			});

			it('has a deleteMemory method', () => {
				expect(manager).to.have.property('deleteMemory').that.is.a('function');
			});

			it('has a getAllMemories method', () => {
				expect(manager).to.have.property('getAllMemories').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(manager).to.have.property('clear').that.is.a('function');
			});
		});
	});

	describe('getMemory()', () => {
		it('returns the correct memory item by unique identifier', () => {
			const result = manager.getMemory('1');
			expect(result).to.deep.equal(memory1);
		});
	});

	describe('setMemory()', () => {
		it('create a new memory unique identifier', () => {
			const newMemory = { unique: 'newMemory', value: 'New Memory' };
			manager.setMemory(newMemory);
			const result = manager.getMemory('newMemory');
			expect(result).to.deep.equal(newMemory);
		});

		it('update an existing memory item by unique identifier', () => {
			const updatedMemory = { unique: '1', value: 'Updated Memory 1' };
			manager.setMemory(updatedMemory);
			const result = manager.getMemory('1');
			expect(result).to.deep.equal(updatedMemory);
		});
	});

	describe('deleteMemory()', () => {
		it('deletes an existing memory item by unique identifier', () => {
			manager.deleteMemory('1');
			const result = manager.getMemory('1');
			expect(result).to.be.undefined;
		});
	});

	describe('getAllMemories()', () => {
		it('returns all memory items', () => {
			const result = manager.getAllMemories();
			expect(result).to.deep.equal([memory1, memory2]);
		});
	});

	describe('clear()', () => {
		it('clears all memory items', () => {
			manager.clear();
			const result = manager.getAllMemories();
			expect(result.length).to.equal(0);
		});
	});
});
