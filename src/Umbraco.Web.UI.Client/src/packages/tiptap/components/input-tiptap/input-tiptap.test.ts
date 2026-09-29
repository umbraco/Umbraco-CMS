import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { manifests as tiptapManifests } from '../../umbraco-package.js';
import { expect, fixture, html, oneEvent, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbServerContext } from '@umbraco-cms/backoffice/server';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import {
	UMB_INTERACTION_MEMORY_SCOPE_CONTEXT,
	UmbInteractionMemoriesChangeEvent,
} from '@umbraco-cms/backoffice/interaction-memory';

describe('UmbInputTiptapElement (standalone)', () => {
	// Proves that `<umb-input-tiptap>` is usable on its own — i.e. not gated on being
	// rendered inside `<umb-property-editor-ui-tiptap>`. We deliberately don't mount
	// the element here: mounting it spins up an `UmbTiptapRteContext` that consumes
	// `UMB_SERVER_CONTEXT`, so a mounted test has to provide a stub — see the
	// 'stylesheets' suite below for that setup.

	it('exports the element class so a standalone consumer can import it', () => {
		expect(UmbInputTiptapElement).to.be.a('function');
	});

	it('registers the `umb-input-tiptap` custom element at module load time', () => {
		expect(customElements.get('umb-input-tiptap')).to.equal(UmbInputTiptapElement);
	});
});

describe('UmbInputTiptapElement (stylesheets)', () => {
	// Regression coverage for #21819: a minimal extension configuration (no extension
	// contributes `getStyles()`) must still load the configured stylesheets, not just
	// Umbraco's own base RTE stylesheet. This is specifically a render-path bug —
	// `#renderStyles()` used to early-return when no extension contributed `getStyles()`,
	// so the `<link>` elements were never emitted at all — so it needs a real mounted
	// element with a minimal extension set to catch it. The stylesheet href resolution
	// itself (root path prefixing, server origin, protocol-relative/absolute URLs) is a
	// pure string transform covered without mounting anything in
	// `resolve-stylesheet-href.function.test.ts`, and the root-path fallback is covered
	// in `tiptap-rte.context.test.ts`.
	//
	// The mount pays for a cold dynamic import of the shared `extension-apis.bundle` chunk
	// (Tiptap core + 50+ extension APIs), which can take several seconds to transform on an
	// uncached test run — hence the generous timeout.

	let provider: UmbContextProvider;

	before(() => {
		umbExtensionsRegistry.registerMany(tiptapManifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(tiptapManifests.map((manifest) => manifest.alias));
	});

	afterEach(() => {
		provider?.hostDisconnected();
	});

	it('loads the configured stylesheet alongside the base RTE stylesheet, even when no enabled extension contributes styles', async function () {
		this.timeout(20000);

		const stub = {
			getHostElement: () => document.body,
			getServerUrl: () => '',
			getServerConnection: () => ({ umbracoCssPath: of('/mycss') }),
		} as unknown as UmbServerContext;
		provider = new UmbContextProvider(document.body, UMB_SERVER_CONTEXT, stub);
		provider.hostConnected();

		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'extensions', value: ['Umb.Tiptap.Bold', 'Umb.Tiptap.Italic'] },
			{ alias: 'stylesheets', value: ['/rte-test.css'] },
		]);

		const element = await fixture<UmbInputTiptapElement>(html`
			<umb-input-tiptap .label=${'Rich Text Editor'} .configuration=${config}></umb-input-tiptap>
		`);

		await waitUntil(() => !!element.shadowRoot?.querySelector('#editor[data-loaded]'), 'editor did not finish loading', {
			timeout: 15000,
		});

		const hrefs = Array.from(element.shadowRoot!.querySelectorAll('link[rel="stylesheet"]')).map((link) =>
			link.getAttribute('href'),
		);

		expect(hrefs).to.include('/umbraco/backoffice/css/rte-content.css');
		expect(hrefs).to.include(`${window.location.origin}/mycss/rte-test.css`);
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
		expect(scope).to.not.equal(undefined);
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
		expect(scope!.getMemory(memory.unique)).to.equal(undefined);
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
