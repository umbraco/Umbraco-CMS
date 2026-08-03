import { umbParseDeprecationOrigin, umbShouldLogDeprecation } from './deprecation-origin.js';
import { expect } from '@open-wc/testing';

// A production-style core URL for the deprecation util itself (so core is recognisable).
const SELF_URL = 'https://example.com/umbraco/backoffice/packages/core/utils/index.js';

describe('umbParseDeprecationOrigin', () => {
	it('returns "unknown" when there is no stack', () => {
		expect(umbParseDeprecationOrigin(undefined, SELF_URL).type).to.equal('unknown');
		expect(umbParseDeprecationOrigin('', SELF_URL).type).to.equal('unknown');
	});

	it('classifies a caller under /App_Plugins/ as a package (Chrome format)', () => {
		const stack = [
			'Error',
			`    at UmbDeprecation.warn (${SELF_URL}:30:10)`,
			'    at new UmbImagingThumbnailElement (https://example.com/umbraco/backoffice/packages/media/index.js:12:5)',
			'    at MyDashboard.render (https://example.com/App_Plugins/Acme.Widgets/dashboard.js:40:9)',
		].join('\n');

		const origin = umbParseDeprecationOrigin(stack, SELF_URL);
		expect(origin.type).to.equal('package');
		expect(origin.label).to.contain('Acme.Widgets');
	});

	it('classifies a caller under /App_Plugins/ as a package (Firefox format)', () => {
		const stack = [
			`warn@${SELF_URL}:30:10`,
			'render@https://example.com/App_Plugins/Cool.Package/index.js:1:1',
		].join('\n');

		const origin = umbParseDeprecationOrigin(stack, SELF_URL);
		expect(origin.type).to.equal('package');
		expect(origin.label).to.contain('Cool.Package');
	});

	it('classifies as "core" when every caller frame is under /umbraco/backoffice/', () => {
		const stack = [
			'Error',
			`    at UmbDeprecation.warn (${SELF_URL}:30:10)`,
			'    at new UmbImagingThumbnailElement (https://example.com/umbraco/backoffice/packages/media/index.js:12:5)',
			'    at UmbMediaCollection.render (https://example.com/umbraco/backoffice/packages/media/index.js:99:3)',
		].join('\n');

		expect(umbParseDeprecationOrigin(stack, SELF_URL).type).to.equal('core');
	});

	it('classifies a non-core, non-App_Plugins caller as "external"', () => {
		const stack = [
			'Error',
			`    at UmbDeprecation.warn (${SELF_URL}:30:10)`,
			'    at Widget.render (https://example.com/my-rcl-assets/widget.js:5:1)',
		].join('\n');

		const origin = umbParseDeprecationOrigin(stack, SELF_URL);
		expect(origin.type).to.equal('external');
		expect(origin.label).to.contain('my-rcl-assets/widget.js');
	});

	it('returns "unknown" for a development build where core is not recognisable', () => {
		const devSelf = 'http://localhost:5173/src/packages/core/utils/deprecation/deprecation.ts';
		const stack = [
			'Error',
			`    at UmbDeprecation.warn (${devSelf}:30:10)`,
			'    at MyDashboard.render (http://localhost:5173/src/my-package/dashboard.ts:40:9)',
		].join('\n');

		expect(umbParseDeprecationOrigin(stack, devSelf).type).to.equal('unknown');
	});
});

describe('umbShouldLogDeprecation', () => {
	it('suppresses core-origin warnings only when suppressCore is true', () => {
		expect(umbShouldLogDeprecation({ type: 'core', label: '' }, true)).to.equal(false);
		expect(umbShouldLogDeprecation({ type: 'core', label: '' }, false)).to.equal(true);
	});

	it('always logs package, external and unknown origins', () => {
		for (const type of ['package', 'external', 'unknown'] as const) {
			expect(umbShouldLogDeprecation({ type, label: '' }, true), type).to.equal(true);
		}
	});
});
