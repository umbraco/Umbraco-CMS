import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from './context/variant.context.js';
import { UmbVariantResolver } from './variant-resolver.js';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-variant-resolver-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface TestVariant {
	culture: string | null;
	segment: string | null;
	name?: string;
}

// Resolves once the observable emits a value matching the predicate.
function observeValue<T>(observable: Observable<T>, predicate: (value: T) => boolean): Promise<T> {
	return new Promise<T>((resolve) => {
		const subscription = observable.subscribe((value) => {
			if (predicate(value)) {
				resolve(value);
				queueMicrotask(() => subscription.unsubscribe());
			}
		});
	});
}

describe('UmbVariantResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let variantContext: UmbVariantContext;
	let controller: UmbVariantResolver<TestVariant>;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		controller = new UmbVariantResolver<TestVariant>(hostElement);

		// Wait until the controller has consumed the variant context.
		await observeValue(controller.displayCulture, (culture) => culture === 'en-US');
		await observeValue(controller.fallbackCulture, (culture) => culture === 'en-US');
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('exposes the display and fallback culture from the variant context', async () => {
		expect(await observeValue(controller.displayCulture, (culture) => culture === 'en-US')).to.equal('en-US');
		expect(await observeValue(controller.fallbackCulture, (culture) => culture === 'en-US')).to.equal('en-US');
	});

	it('resolves the variant matching the display culture', () => {
		controller.setVariants([
			{ culture: 'en-US', segment: null },
			{ culture: 'da-DK', segment: null },
		]);
		expect(controller.getVariant()?.culture).to.equal('en-US');
	});

	it('resolves the invariant variant regardless of culture', () => {
		const invariant: TestVariant = { culture: null, segment: null, name: 'Invariant' };
		controller.setVariants([invariant]);
		expect(controller.getVariant()).to.equal(invariant);
		expect(controller.getFallbackVariant()).to.equal(undefined);
	});

	it('resolves the fallback variant from the fallback culture', async () => {
		await variantContext.setCulture('de-DE');
		await observeValue(controller.displayCulture, (culture) => culture === 'de-DE');

		controller.setVariants([
			{ culture: 'en-US', segment: null },
			{ culture: 'fr-FR', segment: null },
		]);

		// de-DE has no variant, so there is no display match...
		expect(controller.getVariant()).to.equal(undefined);
		// ...but the fallback culture (en-US) does.
		expect(controller.getFallbackVariant()?.culture).to.equal('en-US');
	});

	it('re-resolves when the display culture changes at runtime', async () => {
		controller.setVariants([
			{ culture: 'en-US', segment: null },
			{ culture: 'da-DK', segment: null },
		]);
		expect(controller.getVariant()?.culture).to.equal('en-US');

		await variantContext.setCulture('da-DK');
		await observeValue(controller.variant, (variant) => variant?.culture === 'da-DK');

		expect(controller.getVariant()?.culture).to.equal('da-DK');
	});

	it('exposes the resolved variant culture via the culture observable', async () => {
		controller.setVariants([{ culture: 'en-US', segment: null }]);
		const culture = await observeValue(controller.culture, (value) => value === 'en-US');
		expect(culture).to.equal('en-US');
	});

	it('resolves to undefined when there are no variants', () => {
		controller.setVariants([]);
		expect(controller.getVariant()).to.equal(undefined);
		expect(controller.getFallbackVariant()).to.equal(undefined);
	});

	it('treats undefined variants as an empty set', () => {
		controller.setVariants(undefined);
		expect(controller.getVariant()).to.equal(undefined);
		expect(controller.getVariants()).to.eql([]);
	});
});
