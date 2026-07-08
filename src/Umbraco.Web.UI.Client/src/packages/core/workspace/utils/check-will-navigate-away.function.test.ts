import { umbWorkspaceWillNavigateAway } from './check-will-navigate-away.function.js';
import type { UmbWorkspaceRouteManager } from '../controllers/workspace-route-manager.controller.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { expect } from '@open-wc/testing';

const BASE = '/umbraco/section/content/workspace/document';

function routeManager(config: {
	absolutePath?: string;
	activeLocalPath?: string;
	routes?: Array<Pick<UmbRoute, 'path'>>;
}): UmbWorkspaceRouteManager {
	return {
		getAbsolutePath: () => config.absolutePath,
		getActiveLocalPath: () => config.activeLocalPath ?? '',
		getRoutes: () => (config.routes ?? []) as Array<UmbRoute>,
	} as unknown as UmbWorkspaceRouteManager;
}

describe('umbWorkspaceWillNavigateAway', () => {
	it('does not block before the workspace has been routed (no absolute path)', () => {
		const routes = routeManager({ absolutePath: undefined, activeLocalPath: 'edit/123' });
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/123`)).to.be.false;
	});

	it('stays when navigating to a sub-view of the active route', () => {
		const routes = routeManager({ absolutePath: BASE, activeLocalPath: 'edit/123' });
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/123/view/content`)).to.be.false;
	});

	it('stays when the target is exactly the active route', () => {
		const routes = routeManager({ absolutePath: BASE, activeLocalPath: 'edit/123' });
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/123`)).to.be.false;
	});

	it('navigates away when an identical segment appears elsewhere in the URL (positional anchoring)', () => {
		// The bug: a duplicate `edit/123` deeper in the path must not mask a real change to the owned segment.
		const routes = routeManager({
			absolutePath: BASE,
			activeLocalPath: 'edit/123',
			routes: [{ path: 'edit/:unique' }],
		});
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/456/something/edit/123`)).to.be.true;
	});

	it('does not match a unique that is only a string prefix of the new segment', () => {
		const routes = routeManager({ absolutePath: BASE, activeLocalPath: 'edit/123' });
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/1234`)).to.be.true;
	});

	it('stays through the create -> edit redirect for the freshly created entity', () => {
		const routes = routeManager({
			absolutePath: BASE,
			activeLocalPath: 'create/parent/document-root/null/dt-1',
			routes: [{ path: 'create/parent/:parentEntityType/:parentUnique/:documentTypeUnique' }, { path: 'edit/:unique' }],
		});
		expect(umbWorkspaceWillNavigateAway(routes, 'new-1', `${BASE}/edit/new-1`)).to.be.false;
	});

	it('navigates away when leaving the workspace mount entirely', () => {
		const routes = routeManager({
			absolutePath: BASE,
			activeLocalPath: 'edit/123',
			routes: [{ path: 'edit/:unique' }],
		});
		expect(umbWorkspaceWillNavigateAway(routes, '123', '/umbraco/section/media/workspace/media/edit/123')).to.be.true;
	});

	it('ignores query strings when matching', () => {
		const routes = routeManager({ absolutePath: BASE, activeLocalPath: 'edit/123' });
		expect(umbWorkspaceWillNavigateAway(routes, '123', `${BASE}/edit/123?foo=bar`)).to.be.false;
	});
});
