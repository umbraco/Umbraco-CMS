import { expect } from '@open-wc/testing';
import {
	resolveStaleVariantRoute,
	type UmbStaleVariantRouteResolverVariant,
} from './resolve-stale-variant-route.function.js';
import { UMB_WORKSPACE_PATH_VARIANT_DELIMITER } from './constants.js';

const WORKSPACE_ROUTE = '/section/content/workspace/document/edit/123';

const EN = { culture: 'en-US', segment: null, unique: 'en-US' };
const DA = { culture: 'da', segment: null, unique: 'da' };
const EN_SEG = { culture: 'en-US', segment: 'seg1', unique: 'en-US_seg1' };
const INVARIANT = { culture: null, segment: null, unique: 'invariant' };

function resolve(
	currentPath: string,
	variants: Array<UmbStaleVariantRouteResolverVariant>,
	appCulture: string | undefined = 'en-US',
) {
	return resolveStaleVariantRoute({ currentPath, workspaceRoute: WORKSPACE_ROUTE, variants, appCulture });
}

describe('resolveStaleVariantRoute', () => {
	it('returns null when the variant in the URL exists', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US`, [EN, DA])).to.be.null;
	});

	it('returns null when the variant is valid and a trailing path is present', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US/view/info`, [EN, DA])).to.be.null;
	});

	it('returns null when the current path is outside the workspace route', () => {
		expect(resolve('/section/media/workspace/media/edit/456/invariant', [EN])).to.be.null;
	});

	it('returns null when the current path equals the workspace route (default route case)', () => {
		expect(resolve(WORKSPACE_ROUTE, [EN])).to.be.null;
	});

	it('returns null when the current path is the workspace route with a trailing slash', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/`, [EN])).to.be.null;
	});

	it('falls back to the pure invariant option even when a segment option is listed first', () => {
		const INVARIANT_SEG = { culture: null, segment: 'seg1', unique: 'invariant_seg1' };
		expect(resolve(`${WORKSPACE_ROUTE}/en-US`, [INVARIANT_SEG, INVARIANT])).to.equal(`${WORKSPACE_ROUTE}/invariant`);
	});

	it('returns null while a modal is open on top of the workspace', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/invariant/view/info/modal/umb-modal-workspace/edit/456`, [EN, DA])).to.be.null;
	});

	it('returns null while a modal is open directly on the variant route', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/invariant/modal/umb-modal-workspace/edit/456`, [EN, DA])).to.be.null;
	});

	it('returns null when there are no variant options', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/invariant`, [])).to.be.null;
	});

	it('redirects invariant to the app culture when the type now varies by culture', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/invariant/tab/content`, [EN, DA])).to.equal(
			`${WORKSPACE_ROUTE}/en-US/tab/content`,
		);
	});

	it('falls back to the first option when the app culture is not available', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/invariant`, [DA])).to.equal(`${WORKSPACE_ROUTE}/da`);
	});

	it('redirects a culture variant to invariant when the type no longer varies', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US/view/info`, [INVARIANT])).to.equal(
			`${WORKSPACE_ROUTE}/invariant/view/info`,
		);
	});

	it('collapses a split view when both variants became invalid', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da`, [INVARIANT])).to.equal(
			`${WORKSPACE_ROUTE}/invariant`,
		);
	});

	it('collapses a split view when the invalid side resolves to the valid side', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US${UMB_WORKSPACE_PATH_VARIANT_DELIMITER}da`, [EN])).to.equal(
			`${WORKSPACE_ROUTE}/en-US`,
		);
	});

	it('drops the segment when the segment option no longer exists but the culture does', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US_seg1/view/info`, [EN, DA], 'da')).to.equal(
			`${WORKSPACE_ROUTE}/en-US/view/info`,
		);
	});

	it('returns null for a valid segment variant', () => {
		expect(resolve(`${WORKSPACE_ROUTE}/en-US_seg1`, [EN, EN_SEG])).to.be.null;
	});
});
