import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_INTERACTION_MEMORY_SCOPE_CONTEXT,
	UmbInteractionMemoriesChangeEvent,
} from '@umbraco-cms/backoffice/interaction-memory';

describe('UmbInputTiptapElement (standalone)', () => {
	// Proves that `<umb-input-tiptap>` is usable on its own — i.e. not gated on being
	// rendered inside `<umb-property-editor-ui-tiptap>`. We deliberately don't mount
	// the element here: mounting it spins up an `UmbTiptapRteContext` that consumes
	// `UMB_SERVER_CONTEXT` (not provided in unit tests), and the pending context
	// request would surface as an unhandled rejection after the fixture tears down.
	// The visual end-to-end load path is covered by the Storybook stories instead;
	// this just nails down the contract that standalone consumers depend on.

	it('exports the element class so a standalone consumer can import it', () => {
		expect(UmbInputTiptapElement).to.be.a('function');
	});

	it('registers the `umb-input-tiptap` custom element at module load time', () => {
		expect(customElements.get('umb-input-tiptap')).to.equal(UmbInputTiptapElement);
	});
});

// The interaction-memory wiring only exists once the element's controllers have connected, so this
// block does mount — but skipping `firstUpdated` avoids booting the Tiptap editor, which needs
// extensions from the registry that a unit test does not load. The memory wiring is set up in the
// constructor and is entirely independent of the editor.
@customElement('test-input-tiptap-interaction-memory')
class UmbTestInputTiptapElement extends UmbInputTiptapElement {
	protected override async firstUpdated() {}
}

describe('UmbInputTiptapElement interaction memory', () => {
	let element: UmbTestInputTiptapElement;

	// What the media picker modal stores when reached from the RTE toolbar.
	const memory = {
		unique: 'UmbPickerModal:Umb.Modal.MediaPicker',
		memories: [{ unique: 'UmbMediaItemPickerLocation', value: { entity: { unique: 'folder-1' } } }],
	};

	beforeEach(async () => {
		element = await fixture(html`<test-input-tiptap-interaction-memory></test-input-tiptap-interaction-memory>`);
	});

	it('provides itself as the interaction-memory scope for its modals', async () => {
		const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
		expect(scope).to.not.be.undefined;
	});

	it('makes memories set on the property reachable through the scope', async () => {
		const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
		element.interactionMemories = [memory];
		expect(scope!.getMemory(memory.unique)).to.deep.equal(memory);
	});

	it('drops memories that are no longer present when the property is set again', async () => {
		const scope = (await element.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT))?.memory;
		element.interactionMemories = [memory];
		element.interactionMemories = [];
		expect(scope!.getMemory(memory.unique)).to.be.undefined;
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
		expect(dispatched).to.be.false;
	});
});
