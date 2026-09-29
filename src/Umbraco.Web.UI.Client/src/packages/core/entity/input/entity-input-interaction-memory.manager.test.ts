import { UmbEntityInputInteractionMemoryManager } from './entity-input-interaction-memory.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect, oneEvent } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbInteractionMemoriesChangeEvent, UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('test-entity-input-interaction-memory-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntityInputInteractionMemoryManager', () => {
	let hostElement: UmbTestControllerHostElement;
	let interactionMemory: UmbInteractionMemoryManager;
	let manager: UmbEntityInputInteractionMemoryManager;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		interactionMemory = new UmbInteractionMemoryManager(hostElement);
		manager = new UmbEntityInputInteractionMemoryManager(hostElement, interactionMemory);
	});

	describe('setMemories()', () => {
		it('seeds the interaction memory from the incoming snapshot', () => {
			manager.setMemories([{ unique: 'a' }, { unique: 'b' }]);
			expect(manager.getMemories().map((memory) => memory.unique)).to.eql(['a', 'b']);
		});

		it('removes memories that are no longer present when the snapshot shrinks', () => {
			manager.setMemories([{ unique: 'a' }, { unique: 'b' }]);
			manager.setMemories([{ unique: 'a' }]);
			expect(manager.getMemories().map((memory) => memory.unique)).to.eql(['a']);
		});

		it('clears all memories when the snapshot is emptied', () => {
			manager.setMemories([{ unique: 'a' }, { unique: 'b' }]);
			manager.setMemories([]);
			expect(manager.getMemories()).to.eql([]);
		});

		it('treats undefined as an empty snapshot', () => {
			manager.setMemories([{ unique: 'a' }]);
			manager.setMemories(undefined);
			expect(manager.getMemories()).to.eql([]);
		});
	});

	describe('change event', () => {
		it('dispatches a change event from the host when the interaction memory changes externally', async () => {
			const listener = oneEvent(hostElement, UmbInteractionMemoriesChangeEvent.TYPE);
			interactionMemory.setMemory({ unique: 'expansion', value: { expansion: ['a'] } });
			const event = await listener;
			expect(event).to.exist;
			expect(manager.getMemories().map((memory) => memory.unique)).to.eql(['expansion']);
		});
	});
});
