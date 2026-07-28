import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { manifests as tiptapManifests } from '../../umbraco-package.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbServerContext } from '@umbraco-cms/backoffice/server';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { of } from '@umbraco-cms/backoffice/external/rxjs';

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
	// Umbraco's own base RTE stylesheet. Also covers resolving the configured stylesheet
	// against the server origin, so it doesn't 404 against a split Vite dev client's own origin.
	//
	// The first mount in this suite pays for a cold dynamic import of the shared
	// `extension-apis.bundle` chunk (Tiptap core + 50+ extension APIs), which can take
	// several seconds to transform on an uncached test run — hence the generous timeouts.

	let provider: UmbContextProvider;

	function stubServerContext(cssPath: string | undefined, serverUrl: string) {
		const stub = {
			getHostElement: () => document.body,
			getServerUrl: () => serverUrl,
			getServerConnection: () => ({ umbracoCssPath: of(cssPath) }),
		} as unknown as UmbServerContext;
		provider = new UmbContextProvider(document.body, UMB_SERVER_CONTEXT, stub);
		provider.hostConnected();
	}

	before(() => {
		umbExtensionsRegistry.registerMany(tiptapManifests);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(tiptapManifests.map((manifest) => manifest.alias));
	});

	afterEach(() => {
		provider?.hostDisconnected();
	});

	async function renderElement(cssPath: string | undefined, serverUrl = '', stylesheets: Array<string> = ['/rte-test.css']) {
		stubServerContext(cssPath, serverUrl);

		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'extensions', value: ['Umb.Tiptap.Bold', 'Umb.Tiptap.Italic'] },
			{ alias: 'stylesheets', value: stylesheets },
		]);

		const element = await fixture<UmbInputTiptapElement>(html`
			<umb-input-tiptap .label=${'Rich Text Editor'} .configuration=${config}></umb-input-tiptap>
		`);

		await waitUntil(() => !!element.shadowRoot?.querySelector('#editor[data-loaded]'), 'editor did not finish loading', {
			timeout: 15000,
		});

		return Array.from(element.shadowRoot!.querySelectorAll('link[rel="stylesheet"]')).map((link) =>
			link.getAttribute('href'),
		);
	}

	it('loads the configured stylesheet alongside the base RTE stylesheet, even when no enabled extension contributes styles', async function () {
		this.timeout(20000);
		const hrefs = await renderElement('/mycss');

		expect(hrefs).to.include('/umbraco/backoffice/css/rte-content.css');
		expect(hrefs).to.include('/mycss/rte-test.css');
	});

	it('falls back to the default stylesheet root path when the server reports none', async function () {
		this.timeout(20000);
		const hrefs = await renderElement(undefined);

		expect(hrefs).to.include('/css/rte-test.css');
	});

	it('resolves the configured stylesheet against the server origin, e.g. when running a split Vite dev client', async function () {
		this.timeout(20000);
		const hrefs = await renderElement('/mycss', 'https://localhost:44339');

		expect(hrefs).to.include('https://localhost:44339/mycss/rte-test.css');
		// The base RTE stylesheet is a client-bundled asset (served by Vite itself in dev),
		// so it must stay origin-relative rather than being prefixed with the server URL.
		expect(hrefs).to.include('/umbraco/backoffice/css/rte-content.css');
	});

	it('does not double up the root path when a configured stylesheet already includes it, and still resolves against the server origin', async function () {
		this.timeout(20000);
		const hrefs = await renderElement('/mycss', 'https://localhost:44339', ['/mycss/already-prefixed.css']);

		expect(hrefs).to.include('https://localhost:44339/mycss/already-prefixed.css');
		expect(hrefs).not.to.include('https://localhost:44339/mycss/mycss/already-prefixed.css');
	});
});
