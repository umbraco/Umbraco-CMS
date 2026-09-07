import { UmbInputMultiUrlElement } from './input-multi-url.element.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';
import {
	UMB_INTERACTION_MEMORY_SCOPE_CONTEXT,
	UmbInteractionMemoriesChangeEvent,
} from '@umbraco-cms/backoffice/interaction-memory';

describe('UmbInputMultiUrlElement', () => {
	let element: UmbInputMultiUrlElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-input-multi-url></umb-input-multi-url>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMultiUrlElement);
	});

	// The link picker modal is opened through a modal route, so it is never a descendant of this
	// element — context is the only channel it can read the memories from.
	describe('interaction memory', () => {
		const memory = {
			unique: 'UmbLinkPickerModal',
			memories: [{ unique: 'location', value: { unique: 'folder-1' } }],
		};

		it('provides itself as the interaction-memory scope for its modals', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			expect(scope).to.not.equal(undefined);
		});

		it('makes memories set on the property reachable through the scope', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			element.interactionMemories = [memory];
			expect(scope!.getMemory('UmbLinkPickerModal')).to.deep.equal(memory);
		});

		it('drops memories that are no longer present when the property is set again', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			element.interactionMemories = [memory];
			element.interactionMemories = [];
			expect(scope!.getMemory('UmbLinkPickerModal')).to.equal(undefined);
		});

		it('dispatches interaction-memories-change and exposes the memory when the scope is written to', async () => {
			const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
			const listener = oneEvent(element, UmbInteractionMemoriesChangeEvent.TYPE);
			scope!.setMemory(memory);
			await listener;
			expect(element.interactionMemories).to.deep.equal([memory]);
		});

		it('does not dispatch interaction-memories-change for memories it was just handed', async () => {
			await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT);
			let dispatched = false;
			element.addEventListener(UmbInteractionMemoriesChangeEvent.TYPE, () => (dispatched = true));
			element.interactionMemories = [memory];
			await new Promise((resolve) => setTimeout(resolve, 10));
			expect(dispatched).to.equal(false);
		});
	});
});
