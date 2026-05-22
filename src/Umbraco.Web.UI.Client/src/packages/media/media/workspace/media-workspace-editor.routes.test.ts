import { expect } from '@open-wc/testing';
import { buildMediaWorkspaceRoutes } from './media-workspace-editor.routes.js';
import type { UmbMediaVariantOptionModel } from '../types.js';
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

function makeVariant(unique: string, culture: string | null, segment: string | null = null): UmbMediaVariantOptionModel {
	return { unique, culture, segment } as UmbMediaVariantOptionModel;
}

function findDynamicRoute(routes: Array<UmbRoute>): UmbRoute {
	const route = routes.find((r) => r.path === ':variantPath');
	if (!route) throw new Error('Expected a :variantPath route');
	return route;
}

function callSetup(route: UmbRoute, consumed: string) {
	const info = { match: { fragments: { consumed, rest: '' }, params: {}, match: [] as unknown, route } } as any;
	const setup = (route as unknown as { setup: (component: any, info: any) => void }).setup;
	setup(undefined, info);
}

describe('buildMediaWorkspaceRoutes', () => {
	const splitViewComponent = document.createElement('div');

	describe('production behaviour today (no variants on media types)', () => {
		it('returns only the forbidden catch-all when variants is empty', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});

		it('returns only the forbidden catch-all when no split view manager is available', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView: undefined,
				getIsForbidden: () => false,
			});
			expect(routes).to.have.lengthOf(1);
			expect(routes[0].path).to.equal('**');
		});
	});

	describe('future-proof variant routes', () => {
		it('returns exactly three routes regardless of variant count (regression guard for n²)', () => {
			const small = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			const large = buildMediaWorkspaceRoutes({
				variants: Array.from({ length: 200 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(small.length).to.equal(large.length);
			expect(large.length).to.equal(3);
		});

		it('does not generate any "_&_" route paths', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: Array.from({ length: 50 }, (_v, i) => makeVariant(`v${i}`, `c${i}`)),
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			for (const r of routes) {
				expect(r.path ?? '').to.not.include('_&_');
			}
		});

		it('dynamic route reuses the supplied split-view component instance', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect((findDynamicRoute(routes) as { component?: unknown }).component).to.equal(splitViewComponent);
		});

		it('dynamic route does not set preserveQuery (matches existing media behaviour)', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect((findDynamicRoute(routes) as { preserveQuery?: boolean }).preserveQuery).to.be.oneOf([undefined, false]);
		});

		it('setup treats a single-variant fragment as single-view', () => {
			const splitView = createSplitViewStub();
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView,
				getIsForbidden: () => false,
			});
			callSetup(findDynamicRoute(routes), 'en');
			expect(splitView.calls).to.deep.equal([
				{ method: 'removeActiveVariant', args: [1] },
				{ method: 'handleVariantFolderPart', args: [0, 'en'] },
			]);
		});

		it('setup treats a "_&_" fragment as split-view', () => {
			const splitView = createSplitViewStub();
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				splitViewComponent,
				splitView,
				getIsForbidden: () => false,
			});
			callSetup(findDynamicRoute(routes), 'en_&_da');
			expect(splitView.calls).to.deep.equal([{ method: 'setVariantParts', args: ['en_&_da'] }]);
		});

		it('default route redirects to the first variant', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en'), makeVariant('da', 'da')],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			const defaultRoute = routes.find((r) => r.path === '' && r.pathMatch === 'full');
			expect(defaultRoute).to.exist;
			expect((defaultRoute as { redirectTo?: string }).redirectTo).to.equal('en');
		});

		it('always ends with a "**" catch-all route', () => {
			const routes = buildMediaWorkspaceRoutes({
				variants: [makeVariant('en', 'en')],
				splitViewComponent,
				splitView: createSplitViewStub(),
				getIsForbidden: () => false,
			});
			expect(routes[routes.length - 1].path).to.equal('**');
		});
	});
});
