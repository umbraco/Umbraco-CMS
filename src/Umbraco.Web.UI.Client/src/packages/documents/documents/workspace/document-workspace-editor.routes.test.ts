import { expect } from '@open-wc/testing';
import { buildDocumentWorkspaceRoutes } from './document-workspace-editor.routes.js';
import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

type SplitViewCall = { method: string; args: unknown[] };

const DEFAULT_WORKSPACE_ROUTE = '/workspace/document/edit/x';

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

function createSplitViewComponentStub(): HTMLElement {
	return document.createElement('div');
}

function makeSlot(workspaceRoute: string = DEFAULT_WORKSPACE_ROUTE) {
	return { constructAbsolutePath: () => workspaceRoute };
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

async function callResolve(route: UmbRoute, consumed: string, slot: HTMLElement) {
	const info = {
		slot,
		match: { fragments: { consumed, rest: '' }, params: {}, match: [] as unknown, route },
	} as any;
	const resolve = (route as unknown as { resolve: (info: any) => Promise<void> }).resolve;
	await resolve(info);
}

async function callDefaultResolve(
	route: UmbRoute,
	slot: { constructAbsolutePath: () => string } = makeSlot(),
) {
	const info = { slot, match: { fragments: { consumed: '', rest: '' }, params: {}, match: [] as unknown, route } } as any;
	const resolve = (route as unknown as { resolve: (info: any) => Promise<void> }).resolve;
	await resolve(info);
}

describe('buildDocumentWorkspaceRoutes', () => {
	describe('route count', () => {
		it('returns exactly three routes for a small variant set', () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => DEFAULT_WORKSPACE_ROUTE,
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(3);
		});

		it('returns the same constant route count regardless of variant count (regression guard for n²)', () => {
			const small = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const large = buildDocumentWorkspaceRoutes({
				getVariants: () => Array.from({ length: 200 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				getAppCulture: () => 'v0',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(small.length).to.equal(large.length);
			expect(large.length).to.equal(3);
		});

		it('does not generate any "_&_" route paths', () => {
			const variants = Array.from({ length: 50 }, (_v, i) => makeVariant(`v${i}`, `c${i}`));
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => variants,
				getAppCulture: () => 'v0',
				splitViewComponent: createSplitViewComponentStub(),
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
		let routes: Array<UmbRoute>;
		let dynamic: UmbRoute;

		beforeEach(() => {
			routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			dynamic = findDynamicRoute(routes);
		});

		it('has path ":variantPath"', () => {
			expect(dynamic.path).to.equal(':variantPath');
		});

		it('has preserveQuery true', () => {
			expect((dynamic as { preserveQuery?: boolean }).preserveQuery).to.equal(true);
		});

		it('has a resolve callback (resolver route, not a component route)', () => {
			expect(typeof (dynamic as unknown as { resolve?: unknown }).resolve).to.equal('function');
			expect((dynamic as unknown as { component?: unknown }).component).to.be.undefined;
		});
	});

	describe('resolve callback — valid variants', () => {
		it('mounts the split-view component and calls handleVariantFolderPart for a single-variant path', async () => {
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'en', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
			expect(splitView.calls).to.deep.equal([
				{ method: 'removeActiveVariant', args: [1] },
				{ method: 'handleVariantFolderPart', args: [0, 'en'] },
			]);
		});

		it('mounts the split-view component and calls setVariantParts for a "_&_" path', async () => {
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'en_&_da', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_&_da'] }]);
		});

		it('reuses the split-view component when it is already mounted', async () => {
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			slot.appendChild(splitViewComponent);
			await callResolve(findDynamicRoute(routes), 'da', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
			expect(slot.children).to.have.lengthOf(1);
		});

		it('handles segment-suffixed uniques on both sides of the delimiter', async () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en_segA', 'en', 'segA'), makeVariant('da_segB', 'da', 'segB')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'en_segA_&_da_segB', slot);

			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_segA_&_da_segB'] }]);
		});
	});

	describe('resolve callback — unknown variants (URL is NOT changed)', () => {
		it('mounts UmbRouteNotFoundElement when the consumed fragment is unknown', async () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'nonsense', slot);

			expect((slot.firstChild as Element | null)?.localName).to.equal('umb-route-not-found');
			expect(splitView.calls).to.deep.equal([]);
		});

		it('mounts UmbRouteForbiddenElement when isForbidden is true', async () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => true,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'nonsense', slot);

			expect((slot.firstChild as Element | null)?.localName).to.equal('umb-route-forbidden');
		});

		it('mounts NotFound when only one side of a split-view path is unknown', async () => {
			const splitView = createSplitViewStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'en_&_nonsense', slot);

			expect((slot.firstChild as Element | null)?.localName).to.equal('umb-route-not-found');
			expect(splitView.calls).to.deep.equal([]);
		});

		it('reuses an already-mounted NotFound element', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'nonsense', slot);
			const firstNotFound = slot.firstChild;
			await callResolve(findDynamicRoute(routes), 'still-nonsense', slot);

			expect(slot.firstChild).to.equal(firstNotFound);
		});
	});

	describe('resolvers read variants and appCulture at call time, not at build time', () => {
		// Guards against the umb-router-slot dedup: when route paths are unchanged, the underlying
		// router keeps the original route objects. If callbacks captured variants/appCulture as
		// values, subsequent variant or language changes would be invisible to the resolvers.

		it('dynamic resolver sees a newly added variant', async () => {
			const variants: Array<UmbDocumentVariantOptionModel> = [makeVariant('en', 'en')];
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => variants,
				getAppCulture: () => 'en',
				splitViewComponent,
				splitView,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});

			// Simulate a variant added after the route table is built.
			variants.push(makeVariant('da', 'da'));

			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'da', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
			expect(splitView.calls).to.deep.equal([
				{ method: 'removeActiveVariant', args: [1] },
				{ method: 'handleVariantFolderPart', args: [0, 'da'] },
			]);
		});

		it('default resolver picks the variant matching the current app culture', async () => {
			let appCulture: string | undefined = 'en';
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => appCulture,
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => DEFAULT_WORKSPACE_ROUTE,
				getIsForbidden: () => false,
			});

			const replaceStateCalls: Array<{ url: string }> = [];
			const originalReplaceState = history.replaceState.bind(history);
			history.replaceState = ((_s: unknown, _t: string, url?: string) => {
				replaceStateCalls.push({ url: String(url ?? '') });
			}) as typeof history.replaceState;

			try {
				// Simulate the user switching app language after the routes were built.
				appCulture = 'da';
				await callDefaultResolve(findDefaultRoute(routes));
			} finally {
				history.replaceState = originalReplaceState;
			}

			expect(replaceStateCalls[0].url).to.equal(`${DEFAULT_WORKSPACE_ROUTE}/da`);
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
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => DEFAULT_WORKSPACE_ROUTE,
				getIsForbidden: () => false,
			});
			await callDefaultResolve(findDefaultRoute(routes));

			expect(replaceStateCalls).to.have.lengthOf(1);
			expect(replaceStateCalls[0].url).to.equal(`${DEFAULT_WORKSPACE_ROUTE}/en`);
		});

		it('falls back to the first variant when the app culture has no exact-match unique', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				getAppCulture: () => 'fr',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => DEFAULT_WORKSPACE_ROUTE,
				getIsForbidden: () => false,
			});
			await callDefaultResolve(findDefaultRoute(routes));

			expect(replaceStateCalls[0].url).to.equal(`${DEFAULT_WORKSPACE_ROUTE}/en`);
		});

		it('appends "/view/collection" when ?openCollection is set on the URL', async () => {
			const originalUrl = window.location.href;
			history.pushState({}, '', '?openCollection');
			try {
				const routes = buildDocumentWorkspaceRoutes({
					getVariants: () => [makeVariant('en', 'en')],
					getAppCulture: () => 'en',
					splitViewComponent: createSplitViewComponentStub(),
					splitView: createSplitViewStub(),
					getWorkspaceRoute: () => DEFAULT_WORKSPACE_ROUTE,
					getIsForbidden: () => false,
				});
				await callDefaultResolve(findDefaultRoute(routes));
				expect(replaceStateCalls[0].url).to.equal(`${DEFAULT_WORKSPACE_ROUTE}/en/view/collection`);
			} finally {
				history.pushState({}, '', originalUrl);
			}
		});

		it('returns silently when no workspace route can be determined', async () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => undefined,
				getIsForbidden: () => false,
			});
			await callDefaultResolve(findDefaultRoute(routes), makeSlot(''));

			expect(replaceStateCalls).to.have.lengthOf(0);
		});
	});

	describe('catch-all fallback', () => {
		it('returns only the catch-all when no variants are available', () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the catch-all when no app culture is set yet', () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => undefined,
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the catch-all when no split view manager is available', () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: undefined,
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('always ends with a "**" catch-all route', () => {
			const routes = buildDocumentWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				getAppCulture: () => 'en',
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getWorkspaceRoute: () => '/x',
				getIsForbidden: () => false,
			});
			expect(routes[routes.length - 1].path).to.equal('**');
		});
	});
});
