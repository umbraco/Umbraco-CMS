import { expect } from '@open-wc/testing';
import { resolveStaleVariantRoute } from './resolve-stale-variant-route.function.js';
import { UMB_WORKSPACE_PATH_VARIANT_DELIMITER } from './constants.js';

const WORKSPACE_ROUTE = '/section/content/workspace/document/edit/123';

const EN = { culture: 'en-US', segment: null, unique: 'en-US' };
const DA = { culture: 'da', segment: null, unique: 'da' };
const EN_SEG = { culture: 'en-US', segment: 'seg1', unique: 'en-US_seg1' };
const INVARIANT = { culture: null, segment: null, unique: 'invariant' };

describe('resolveStaleVariantRoute', () => {
	it('returns null when the variant in the URL exists', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN, DA],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('returns null when the variant is valid and a trailing path is present', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US/view/info`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN, DA],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('returns null when the current path is outside the workspace route', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: '/section/media/workspace/media/edit/456/invariant',
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('returns null when the current path equals the workspace route (default route case)', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: WORKSPACE_ROUTE,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('returns null when the current path is the workspace route with a trailing slash', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('falls back to the pure invariant option even when a segment option is listed first', () => {
		const INVARIANT_SEG = { culture: null, segment: 'seg1', unique: 'invariant_seg1' };
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [INVARIANT_SEG, INVARIANT],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/invariant`);
	});

	it('returns null when there are no variant options', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/invariant`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});

	it('redirects invariant to the app culture when the type now varies by culture', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/invariant/tab/content`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN, DA],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/en-US/tab/content`);
	});

	it('falls back to the first option when the app culture is not available', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/invariant`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [DA],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/da`);
	});

	it('redirects a culture variant to invariant when the type no longer varies', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US/view/info`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [INVARIANT],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/invariant/view/info`);
	});

	it('collapses a split view when both variants became invalid', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [INVARIANT],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/invariant`);
	});

	it('collapses a split view when the invalid side resolves to the valid side', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN],
				appCulture: 'en-US',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/en-US`);
	});

	it('drops the segment when the segment option no longer exists but the culture does', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US_seg1/view/info`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN, DA],
				appCulture: 'da',
			}),
		).to.equal(`${WORKSPACE_ROUTE}/en-US/view/info`);
	});

	it('returns null for a valid segment variant', () => {
		expect(
			resolveStaleVariantRoute({
				currentPath: `${WORKSPACE_ROUTE}/en-US_seg1`,
				workspaceRoute: WORKSPACE_ROUTE,
				variants: [EN, EN_SEG],
				appCulture: 'en-US',
			}),
		).to.be.null;
	});
});
