import { expect } from '@open-wc/testing';
import { buildDocumentWorkspaceRoutes } from './document-workspace-editor.routes.js';
import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

type SplitViewCall = { method: string; args: unknown[] };

function createSplitViewStub() {
	const calls: SplitViewCall[] = [];
	const stub = {
		calls,
		setVariantParts: (...args: unknown[]) => calls.push({ method: 'setVariantParts', args }),
		handleVariantFolderPart: (...args: unknown[]) => calls.push({ method: 'handleVariantFolderPart', args }),
		removeActiveVariant: (...args: unknown[]) => calls.push({ method: 'removeActiveVariant', args }),
	};
	return stub as unknown as UmbWorkspaceSplitViewManager & { calls: SplitViewCall[] };
}

function makeVariant(unique: string, culture: string | null, segment: string | null = null): UmbDocumentVariantOptionModel {
	return { unique, culture, segment } as UmbDocumentVariantOptionModel;
}

function findDynamicRoute(routes: Array<UmbRoute>): UmbRoute {
	const route = routes.find((r) => r.path === ':variantPath');
	if (!route) throw new Error('Expected a :variantPath route');
	return route;
}

function findDefaultRoute(routes: Array<UmbRoute>): UmbRoute {
	const route = routes.find((r) => r.path === '' && r.pathMatch === 'full');
	if (!route) throw new Error('Expected an empty-path full-match default route');
	return route;
}

function callSetup(route: UmbRoute, consumed: string) {
	const info = { match: { fragments: { consumed, rest: '' }, params: {}, match: [] as unknown, route } } as any;
	const setup = (route as unknown as { setup: (component: any, info: any) => void }).setup;
	setup(undefined, info);
}

describe('buildDocumentWorkspaceRoutes', () => {
	const splitViewComponent = document.createElement('div');

	describe('route count', () => {
		it('returns exactly three routes for a small variant set', () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/workspace/document/edit/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(3);
		});

		it('returns the same constant route count regardless of variant count (regression guard for n²)', () => {
			const small = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const large = buildDocumentWorkspaceRoutes({
				variants: Array.from({ length: 200 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				appCulture: 'v0',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(small.length).to.equal(large.length);
			expect(large.length).to.equal(3);
		});

		it('does not generate any "_&_" route paths (regression guard for split-view pairs)', () => {
			const variants = Array.from({ length: 50 }, (_v, i) => makeVariant(`v${i}`, `c${i}`));
			const routes = buildDocumentWorkspaceRoutes({
				variants,
				appCulture: 'v0',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			for (const r of routes) {
				expect(r.path ?? '').to.not.include('_&_');
			}
		});
	});

	describe('dynamic route shape', () => {
		const routes = buildDocumentWorkspaceRoutes({
			variants: [makeVariant('en', 'en')],
			appCulture: 'en',
			splitViewComponent,
			splitView: createSplitViewStub(),
			getWorkspaceRoute: () => '/x',
			getIsForbidden: () => false,
		});
		const dynamic = findDynamicRoute(routes);

		it('has path ":variantPath"', () => {
			expect(dynamic.path).to.equal(':variantPath');
		});

		it('reuses the supplied split-view component instance', () => {
			expect((dynamic as { component?: unknown }).component).to.equal(splitViewComponent);
		});

		it('has preserveQuery true', () => {
			expect((dynamic as { preserveQuery?: boolean }).preserveQuery).to.equal(true);
		});

		it('has a setup callback', () => {
			expect(typeof (dynamic as unknown as { setup?: unknown }).setup).to.equal('function');
		});
	});

	describe('setup callback', () => {
		it('treats a single-variant fragment as single-view (removeActiveVariant(1) then handleVariantFolderPart(0, fragment))', () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			callSetup(findDynamicRoute(routes), 'en');

			expect(splitView.calls).to.deep.equal([
				{ method: 'removeActiveVariant', args: [1] },
				{ method: 'handleVariantFolderPart', args: [0, 'en'] },
			]);
		});

		it('treats a "_&_" fragment as split-view (setVariantParts with the consumed fragment)', () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				appCulture: 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			callSetup(findDynamicRoute(routes), 'en_&_da');

			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_&_da'] }]);
		});

		it('handles segment-suffixed uniques on both sides of the delimiter', () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en_segA', 'en', 'segA'), makeVariant('da_segB', 'da', 'segB')],
				appCulture: 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			callSetup(findDynamicRoute(routes), 'en_segA_&_da_segB');

			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_segA_&_da_segB'] }]);
		});
	});

	describe('default route resolve', () => {
		let originalReplaceState: typeof history.replaceState;
		let replaceStateCalls: Array<{ url: string }>;

		beforeEach(() => {
			replaceStateCalls = [];
			originalReplaceState = history.replaceState.bind(history);
			history.replaceState = ((_state: unknown, _title: string, url?: string) => {
				replaceStateCalls.push({ url: String(url ?? '') });
			}) as typeof history.replaceState;
		});

		afterEach(() => {
			history.replaceState = originalReplaceState;
		});

		it('redirects to the variant whose unique matches the app culture', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/workspace/document/edit/x',
				getIsForbidden: () => false,
			});
			await (findDefaultRoute(routes) as unknown as { resolve: () => Promise<void> }).resolve();

			expect(replaceStateCalls).to.have.lengthOf(1);
			expect(replaceStateCalls[0].url).to.equal('/workspace/document/edit/x/en');
		});

		it('falls back to the first variant when the app culture has no exact-match unique', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				appCulture: 'fr',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/workspace/document/edit/x',
				getIsForbidden: () => false,
			});
			await (findDefaultRoute(routes) as unknown as { resolve: () => Promise<void> }).resolve();

			expect(replaceStateCalls[0].url).to.equal('/workspace/document/edit/x/en');
		});

		it('falls back to the first variant when only segment-suffixed variants exist for the app culture', async () => {
			// app culture "en" but there is no variant with unique==="en" — only "en_segA".
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en_segA', 'en', 'segA'), makeVariant('da', 'da')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/workspace/document/edit/x',
				getIsForbidden: () => false,
			});
			await (findDefaultRoute(routes) as unknown as { resolve: () => Promise<void> }).resolve();

			expect(replaceStateCalls[0].url).to.equal('/workspace/document/edit/x/en_segA');
		});

		it('appends "/view/collection" when ?openCollection is set on the URL', async () => {
			// Use the real history.pushState to put ?openCollection on the URL; history.replaceState
			// is stubbed in beforeEach so the resolve() call won't actually navigate.
			const originalUrl = window.location.href;
			history.pushState({}, '', '?openCollection');
			try {
				const routes = buildDocumentWorkspaceRoutes({
					variants: [makeVariant('en', 'en')],
					appCulture: 'en',
					splitViewComponent,
					splitView: createSplitViewStub(),
					getWorkspaceRoute: () => '/workspace/document/edit/x',
					getIsForbidden: () => false,
				});
				await (findDefaultRoute(routes) as unknown as { resolve: () => Promise<void> }).resolve();
				expect(replaceStateCalls[0].url).to.equal('/workspace/document/edit/x/en/view/collection');
			} finally {
				history.pushState({}, '', originalUrl);
			}
		});

		it('throws when no workspace route is available', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => undefined,
				getIsForbidden: () => false,
			});
			let threw: unknown = null;
			try {
				await (findDefaultRoute(routes) as unknown as { resolve: () => Promise<void> }).resolve();
			} catch (e) {
				threw = e;
			}
			expect(threw).to.be.an.instanceOf(Error);
		});
	});

	describe('catch-all fallback', () => {
		it('returns only the forbidden catch-all when no variants are available', () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the forbidden catch-all when no app culture is set yet', () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: undefined,
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the forbidden catch-all when no split view manager is available', () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: 'en',
				splitViewComponent,
				splitView: undefined,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('always ends with a "**" catch-all route', () => {
			const routes = buildDocumentWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				appCulture: 'en',
				splitViewComponent,
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes[routes.length - 1].path).to.equal('**');
		});
	});
});
