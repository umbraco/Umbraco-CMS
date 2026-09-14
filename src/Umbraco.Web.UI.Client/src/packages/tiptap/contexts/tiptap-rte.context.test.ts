import { UmbTiptapRteContext } from './tiptap-rte.context.js';
import { expect } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { filter, firstValueFrom, of } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbServerContext } from '@umbraco-cms/backoffice/server';

// Regression coverage for #21819: the server-resolved stylesheet root path must fall back
// to a sane default rather than being left `undefined` for the whole session, which is what
// dropped configured stylesheets on `v17/dev` when the server never reported `umbracoCssPath`.

@customElement('umb-test-tiptap-rte-context-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

// `stylesheetRootPath` starts as `undefined` and only resolves once the async context-request
// for `UMB_SERVER_CONTEXT` completes, so wait for the first defined emission rather than the
// first emission outright.
function firstResolvedRootPath(context: UmbTiptapRteContext) {
	return firstValueFrom(context.stylesheetRootPath.pipe(filter((value) => value !== undefined)));
}

describe('UmbTiptapRteContext', () => {
	let host: UmbTestHostElement;
	let provider: UmbContextProvider;

	function stubServerContext(cssPath: string | undefined) {
		const stub = {
			getHostElement: () => document.body,
			getServerUrl: () => '',
			getServerConnection: () => ({ umbracoCssPath: of(cssPath) }),
		} as unknown as UmbServerContext;
		provider = new UmbContextProvider(document.body, UMB_SERVER_CONTEXT, stub);
		provider.hostConnected();
	}

	beforeEach(() => {
		host = new UmbTestHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		provider?.hostDisconnected();
		host.remove();
	});

	it('reports the server-configured stylesheet root path', async () => {
		stubServerContext('/mycss');
		const context = new UmbTiptapRteContext(host);

		expect(await firstResolvedRootPath(context)).to.equal('/mycss');
	});

	it('falls back to the default root path when the server reports none', async () => {
		stubServerContext(undefined);
		const context = new UmbTiptapRteContext(host);

		expect(await firstResolvedRootPath(context)).to.equal('/css');
	});

	it('falls back to the default root path when there is no server connection at all', async () => {
		const stub = {
			getHostElement: () => document.body,
			getServerUrl: () => '',
			getServerConnection: () => undefined,
		} as unknown as UmbServerContext;
		provider = new UmbContextProvider(document.body, UMB_SERVER_CONTEXT, stub);
		provider.hostConnected();
		const context = new UmbTiptapRteContext(host);

		expect(await firstResolvedRootPath(context)).to.equal('/css');
	});
});
