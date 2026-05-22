import { expect } from '@open-wc/testing';
import { buildMediaWorkspaceRoutes } from './media-workspace-editor.routes.js';
import type { UmbMediaVariantOptionModel } from '../types.js';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

type SplitViewCall = { method: string; args: unknown[] };

const DEFAULT_WORKSPACE_ROUTE = '/workspace/media/edit/x';

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

function makeVariant(unique: string, culture: string | null, segment: string | null = null): UmbMediaVariantOptionModel {
	return { unique, culture, segment } as UmbMediaVariantOptionModel;
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

describe('buildMediaWorkspaceRoutes', () => {
	describe('production behaviour today (no variants on media types)', () => {
		it('returns only the catch-all when variants is empty', () => {
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [],
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the catch-all when no split view manager is available', () => {
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent: createSplitViewComponentStub(),
				splitView: undefined,
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});
	});

	describe('future-proof variant routes', () => {
		it('returns the same constant route count regardless of variant count (regression guard for n²)', () => {
			const small = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			const large = buildMediaWorkspaceRoutes({
				getVariants: () => Array.from({ length: 200 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(small.length).to.equal(large.length);
			expect(large.length).to.equal(3);
		});

		it('does not generate any "_&_" route paths', () => {
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => Array.from({ length: 50 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			for (const r of routes) {
				expect(r.path ?? '').to.not.include('_&_');
			}
		});

		it('dynamic route is a resolver (has resolve, no component)', () => {
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			const dynamic = findDynamicRoute(routes);
			expect(typeof (dynamic as unknown as { resolve?: unknown }).resolve).to.equal('function');
			expect((dynamic as unknown as { component?: unknown }).component).to.be.undefined;
		});

		it('mounts the split-view component for a single-variant path', async () => {
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent,
				splitView,
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

		it('mounts the split-view component for a "_&_" path', async () => {
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
				splitViewComponent,
				splitView,
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'en_&_da', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_&_da'] }]);
		});

		it('mounts UmbRouteNotFoundElement for an unknown variant (URL is not changed)', async () => {
			const splitView = createSplitViewStub();
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent: createSplitViewComponentStub(),
				splitView,
				getIsForbidden: () => false,
			});
			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'nonsense', slot);

			expect((slot.firstChild as Element | null)?.localName).to.equal('umb-route-not-found');
			expect(splitView.calls).to.deep.equal([]);
		});

		it('default route resolves to the first variant', async () => {
			const replaceStateCalls: Array<{ url: string }> = [];
			const originalReplaceState = history.replaceState.bind(history);
			history.replaceState = ((_s: unknown, _t: string, url?: string) => {
				replaceStateCalls.push({ url: String(url ?? '') });
			}) as typeof history.replaceState;

			try {
				const routes = buildMediaWorkspaceRoutes({
					getVariants: () => [makeVariant('en', 'en'), makeVariant('da', 'da')],
					splitViewComponent: createSplitViewComponentStub(),
					splitView: createSplitViewStub(),
					getIsForbidden: () => false,
				});
				await callDefaultResolve(findDefaultRoute(routes));
			} finally {
				history.replaceState = originalReplaceState;
			}

			expect(replaceStateCalls[0].url).to.equal(`${DEFAULT_WORKSPACE_ROUTE}/en`);
		});

		it('default route returns silently when no workspace route can be determined', async () => {
			const replaceStateCalls: Array<{ url: string }> = [];
			const originalReplaceState = history.replaceState.bind(history);
			history.replaceState = ((_s: unknown, _t: string, url?: string) => {
				replaceStateCalls.push({ url: String(url ?? '') });
			}) as typeof history.replaceState;

			try {
				const routes = buildMediaWorkspaceRoutes({
					getVariants: () => [makeVariant('en', 'en')],
					splitViewComponent: createSplitViewComponentStub(),
					splitView: createSplitViewStub(),
					getIsForbidden: () => false,
				});
				await callDefaultResolve(findDefaultRoute(routes), makeSlot(''));
			} finally {
				history.replaceState = originalReplaceState;
			}

			expect(replaceStateCalls).to.have.lengthOf(0);
		});

		it('always ends with a "**" catch-all route', () => {
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => [makeVariant('en', 'en')],
				splitViewComponent: createSplitViewComponentStub(),
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(routes[routes.length - 1].path).to.equal('**');
		});
	});

	describe('resolvers read variants at call time (regression guard for stale closure)', () => {
		it('dynamic resolver sees a newly added variant', async () => {
			const variants: Array<UmbMediaVariantOptionModel> = [makeVariant('en', 'en')];
			const splitView = createSplitViewStub();
			const splitViewComponent = createSplitViewComponentStub();
			const routes = buildMediaWorkspaceRoutes({
				getVariants: () => variants,
				splitViewComponent,
				splitView,
				getIsForbidden: () => false,
			});

			variants.push(makeVariant('da', 'da'));

			const slot = document.createElement('div');
			await callResolve(findDynamicRoute(routes), 'da', slot);

			expect(slot.firstChild).to.equal(splitViewComponent);
		});
	});
});
