import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { expect } from '@open-wc/testing';

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
