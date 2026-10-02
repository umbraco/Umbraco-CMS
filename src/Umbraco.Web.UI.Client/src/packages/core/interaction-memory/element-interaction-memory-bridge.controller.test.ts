import { UmbElementInteractionMemoryBridgeController } from './element-interaction-memory-bridge.controller.js';
import { UmbInteractionMemoriesChangeEvent } from './event/interaction-memories-change.event.js';
import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect, oneEvent } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-element-interaction-memory-bridge-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbElementInteractionMemoryBridgeController', () => {
	let hostElement: UmbTestControllerHostElement;
	let interactionMemory: UmbInteractionMemoryManager;
	let bridge: UmbElementInteractionMemoryBridgeController;
	let snapshots: Array<Array<string>>;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		interactionMemory = new UmbInteractionMemoryManager(hostElement);
		bridge = new UmbElementInteractionMemoryBridgeController(hostElement, interactionMemory);
		snapshots = [];
		hostElement.addEventListener(UmbInteractionMemoriesChangeEvent.TYPE, () =>
			snapshots.push(bridge.getMemories().map((memory) => memory.unique)),
		);
	});

	describe('setMemories()', () => {
		it('applies the snapshot without dispatching a change event', () => {
			bridge.setMemories([{ unique: 'a' }, { unique: 'b' }, { unique: 'c' }]);
			expect(bridge.getMemories().map((memory) => memory.unique)).to.eql(['a', 'b', 'c']);
			expect(snapshots).to.eql([]);
		});

		it('applies a snapshot that both removes and adds memories in one go', () => {
			bridge.setMemories([{ unique: 'a' }, { unique: 'b' }]);
			bridge.setMemories([{ unique: 'c' }]);
			expect(bridge.getMemories().map((memory) => memory.unique)).to.eql(['c']);
			expect(snapshots).to.eql([]);
		});
	});

	describe('change event', () => {
		it('dispatches a change event when the bridged manager changes', async () => {
			const listener = oneEvent(hostElement, UmbInteractionMemoriesChangeEvent.TYPE);
			interactionMemory.setMemory({ unique: 'a' });
			await listener;
			expect(snapshots).to.eql([['a']]);
		});

		it('dispatches a single change event when the manager changes after a snapshot was applied', async () => {
			bridge.setMemories([{ unique: 'a' }, { unique: 'b' }]);
			const listener = oneEvent(hostElement, UmbInteractionMemoriesChangeEvent.TYPE);
			interactionMemory.setMemory({ unique: 'c' });
			await listener;
			expect(snapshots).to.eql([['a', 'b', 'c']]);
		});
	});
});
