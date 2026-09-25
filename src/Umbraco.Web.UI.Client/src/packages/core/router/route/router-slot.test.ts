import { expect, fixture, html, aTimeout, waitUntil } from '@open-wc/testing';
import type { UmbRoute, UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import type { IRoute, IRouterSlot } from '@umbraco-cms/backoffice/router';

import '@umbraco-cms/backoffice/router';

/**
 * Covers what happens to an already-matched router-slot when its route array is later
 * REPLACED - as opposed to the last describe block below, which covers only the timing
 * of a slot's initial mount.
 */
describe('UmbRouterSlotElement - routes setter diff', () => {
	function createMarkedRoute(path: string, unique: string | undefined, marker: string): UmbRoute {
		return {
			path,
			unique,
			component: () => document.createElement('div'),
			setup: (component: HTMLElement | undefined) => component?.setAttribute('data-marker', marker),
		} as UmbRoute;
	}

	function getMarker(root: UmbRouterSlotElement): string | null {
		return root.shadowRoot!.querySelector('[data-marker]')?.getAttribute('data-marker') ?? null;
	}

	it('re-renders a same-length, same-path-set replacement whose route unique changes', async () => {
		const root = await fixture<UmbRouterSlotElement>(html`<umb-router-slot></umb-router-slot>`);
		root.routes = [createMarkedRoute('', 'a', 'a')];
		await aTimeout(0);
		expect(getMarker(root), 'should have rendered the "a" route').to.equal('a');

		// Same length (1), same path set (['']), only `unique` (and the component it targets) changed.
		root.routes = [createMarkedRoute('', 'b', 'b')];
		await aTimeout(0);

		expect(getMarker(root), 'should have re-rendered once `unique` changed').to.equal('b');
	});

	it('stays a no-op for a same-length, same-path-set replacement whose unique is unchanged', async () => {
		const root = await fixture<UmbRouterSlotElement>(html`<umb-router-slot></umb-router-slot>`);
		root.routes = [createMarkedRoute('', undefined, 'a')];
		await aTimeout(0);
		expect(getMarker(root), 'should have rendered the "a" route').to.equal('a');

		// Same length, same path set, `unique` unchanged (undefined on both sides) - only the
		// component differs. This must stay a no-op: it's not the diff's job to detect this,
		// only `unique` is the documented signal for "this path-stable route now differs".
		root.routes = [createMarkedRoute('', undefined, 'b')];
		await aTimeout(0);

		expect(getMarker(root), 'should not have re-rendered - unique did not change').to.equal('a');
	});
});

/**
 * Covers what happens to an already-matched `router-slot` when its route array is later
 * REPLACED - exercising `shouldNavigate`/`matchRoutes`/guards/redirects directly, rather than
 * the `UmbRouterSlotElement` wrapper's diff (covered above). Since none of that matching logic
 * branches on whether the slot is root or nested, these use a plain root `router-slot`.
 */
describe('RouterSlot - route array changes', () => {
	const ORIGINAL_PATH = window.location.pathname;

	afterEach(() => {
		history.replaceState(null, '', ORIGINAL_PATH);
	});

	function createRouterSlot(): IRouterSlot {
		return document.createElement('router-slot') as unknown as IRouterSlot;
	}

	function createComponentRoute(path: string, extra?: Partial<IRoute>): IRoute {
		return { path, component: () => document.createElement('div'), ...extra } as IRoute;
	}

	it('picks up a match once routes are replaced twice, first with a non-matching set, then a matching one', async () => {
		history.replaceState(null, '', '/general');

		const root = createRouterSlot();
		document.body.appendChild(root);
		root.routes = [];
		await aTimeout(0);
		expect(root.match, 'should not match with no routes').to.be.null;

		// An update arrives, but it doesn't cover the currently requested path yet.
		root.routes = [createComponentRoute('other')];
		await aTimeout(0);
		expect(root.match, 'should still not match - "general" is not among the routes yet').to.be.null;

		// A further update finally includes the requested path.
		root.routes = [createComponentRoute('other'), createComponentRoute('general')];
		await aTimeout(0);

		expect(root.match?.route.path).to.equal('general');
	});

	it('re-renders when an empty-path route matches, then is replaced by one with a different component and unique', async () => {
		history.replaceState(null, '', '/anything');

		const root = createRouterSlot();
		document.body.appendChild(root);
		const pageA = document.createElement('div');
		root.routes = [{ path: '', unique: 'a', component: () => pageA } as IRoute];
		await aTimeout(0);
		expect(root.firstChild, 'should have rendered page A').to.equal(pageA);

		const pageB = document.createElement('div');
		root.routes = [{ path: '', unique: 'b', component: () => pageB } as IRoute];
		await aTimeout(0);

		expect(root.match?.route.unique, 're-matched route should carry the new unique').to.equal('b');
		expect(root.firstChild, 'should have replaced page A with page B').to.equal(pageB);
	});

	it('keeps the active route mounted when routes are added or removed elsewhere in the array', async () => {
		history.replaceState(null, '', '/tab-a');

		const root = createRouterSlot();
		document.body.appendChild(root);
		const activePage = document.createElement('div');
		root.routes = [createComponentRoute('tab-a', { component: () => activePage } as Partial<IRoute>)];
		await aTimeout(0);
		expect(root.firstChild, 'should have rendered the active tab').to.equal(activePage);

		// A second tab is added elsewhere in the array - the active tab is untouched.
		root.routes = [
			createComponentRoute('tab-a', { component: () => activePage } as Partial<IRoute>),
			createComponentRoute('tab-b'),
		];
		await aTimeout(0);
		expect(root.firstChild, 'active tab should keep its component instance after a route is added').to.equal(
			activePage,
		);

		// The newly-added tab is removed again.
		root.routes = [createComponentRoute('tab-a', { component: () => activePage } as Partial<IRoute>)];
		await aTimeout(0);
		expect(root.firstChild, 'active tab should keep its component instance after a route is removed').to.equal(
			activePage,
		);
	});

	it('lets ordering, not the path set, decide between an overlapping param route and a literal route', async () => {
		history.replaceState(null, '', '/general');

		const root = createRouterSlot();
		document.body.appendChild(root);

		// The param route is listed first, so it wins even though "general" also matches literally.
		root.routes = [createComponentRoute(':name'), createComponentRoute('general')];
		await aTimeout(0);
		expect(root.match?.route.path, 'the param route should win when listed first').to.equal(':name');

		// Reordering with the exact same path set changes which route matches.
		root.routes = [createComponentRoute('general'), createComponentRoute(':name')];
		await aTimeout(0);
		expect(root.match?.route.path, 'the literal route should win once it is listed first').to.equal('general');
	});

	it('cancels the navigation outright when a guard on the catch-all route rejects it, rather than falling through', async () => {
		history.replaceState(null, '', '/anything');

		const root = createRouterSlot();
		document.body.appendChild(root);
		root.routes = [createComponentRoute('**', { guards: [() => false] } as Partial<IRoute>)];
		await aTimeout(0);

		expect(root.match, 'a rejected guard should cancel the navigation, not match anyway').to.be.null;
		expect(root.firstChild, 'nothing should have been rendered').to.be.null;
	});

	it('never applies a redirect introduced on a path that already matched a different, non-redirect route', async () => {
		history.replaceState(null, '', '/general');

		const root = createRouterSlot();
		document.body.appendChild(root);
		const page = document.createElement('div');
		root.routes = [{ path: 'general', component: () => page } as IRoute];
		await aTimeout(0);
		expect(root.firstChild, 'should have rendered the original page').to.equal(page);

		// Same path, same consumed fragment, same (absent) unique - only `redirectTo` was added.
		root.routes = [{ path: 'general', redirectTo: 'elsewhere' } as IRoute];
		await aTimeout(0);

		expect(window.location.pathname, 'the newly-added redirect should never have fired').to.equal('/general');
		expect(root.firstChild, 'the original page should still be mounted').to.equal(page);
	});
});

/**
 * Covers the timing of a slot's initial mount, nested inside the component that a parent
 * route resolves to (never appended to the parent slot directly - matching how a workspace
 * view nests its own `umb-router-slot` inside the component the workspace-editor's router
 * resolves it to).
 */
describe('RouterSlot - mount-time recovery (nested, non-root)', () => {
	const ORIGINAL_PATH = window.location.pathname;

	afterEach(() => {
		history.replaceState(null, '', ORIGINAL_PATH);
	});

	function createRouterSlot(): IRouterSlot {
		return document.createElement('router-slot') as unknown as IRouterSlot;
	}

	function createComponentRoute(path: string): IRoute {
		return { path, component: () => document.createElement('div') } as IRoute;
	}

	/** A route whose resolved "page" is a plain element with the given child nested inside it. */
	function createParentRouteWrapping(child: IRouterSlot): IRoute {
		return {
			path: 'section',
			component: () => {
				const page = document.createElement('div');
				page.appendChild(child as unknown as Node);
				return page;
			},
		} as IRoute;
	}

	// Be aware this case is unlikely to happen as most routes will create a new router-slot inside of it. [NL]
	it('eventually matches if it connects already holding its final routes, as part of an already-matched parent route', async () => {
		history.replaceState(null, '', '/section/general');

		// The child is fully assembled before it ever connects: its routes are already correct,
		// mirroring the guarded `${this._routes ? html`<umb-router-slot ...>` : nothing}` pattern.
		const child = createRouterSlot();
		child.routes = [createComponentRoute('general')];

		const root = createRouterSlot();
		document.body.appendChild(root);
		root.routes = [createParentRouteWrapping(child)];

		// The recovery for a slot connecting under an already-matched parent runs on a
		// requestAnimationFrame scheduled from connectedCallback, so it is not synchronous with
		// connecting - give it a chance to run.
		await waitUntil(() => child.match !== null, 'child never matched after connecting');
		expect(child.match?.route.path).to.equal('general');
	});
});
