import { UmbDocumentCurrentVariantResolver } from './document-current-variant-resolver.js';
import { UmbDocumentVariantState } from '../variant-state.js';
import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-document-current-variant-resolver-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface TestVariant {
	name: string;
	culture: string | null;
	segment?: string | null;
	state: UmbDocumentVariantState | null;
	flags: Array<UmbEntityFlag>;
}

function makeVariant(overrides: Partial<TestVariant> = {}): TestVariant {
	return {
		name: 'Test',
		culture: 'en-US',
		segment: null,
		state: UmbDocumentVariantState.PUBLISHED,
		flags: [],
		...overrides,
	};
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

describe('UmbDocumentCurrentVariantResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let variantContext: UmbVariantContext;
	let resolver: UmbDocumentCurrentVariantResolver<TestVariant>;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		resolver = new UmbDocumentCurrentVariantResolver<TestVariant>(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('currentVariant', () => {
		it('resolves to the variant matching the display culture', async () => {
			const enVariant = makeVariant({ culture: 'en-US' });
			resolver.setVariants([enVariant, makeVariant({ culture: 'da-DK' })], undefined);

			const currentVariant = await observeValue(resolver.currentVariant, (variant) => variant !== undefined);
			expect(currentVariant).to.equal(enVariant);
		});

		it('falls back to the fallback culture variant when the display culture has none', async () => {
			await variantContext.setCulture('de-DE');
			// fallbackCulture stays 'en-US' from the outer beforeEach.

			const fallbackVariant = makeVariant({ culture: 'en-US' });
			resolver.setVariants([fallbackVariant], undefined);

			const currentVariant = await observeValue(resolver.currentVariant, (variant) => variant !== undefined);
			expect(currentVariant).to.equal(fallbackVariant);
		});

		it('is undefined when neither the display nor the fallback culture has a matching variant', async () => {
			await variantContext.setCulture('de-DE');
			await variantContext.setFallbackCulture('fr-FR');

			resolver.setVariants([makeVariant({ culture: 'en-US' })], undefined);

			expect(resolver.getCurrentVariant()).to.equal(undefined);
		});

		it('updates when the display culture changes at runtime', async () => {
			const enVariant = makeVariant({ culture: 'en-US' });
			const daVariant = makeVariant({ culture: 'da-DK' });
			resolver.setVariants([enVariant, daVariant], undefined);
			await observeValue(resolver.currentVariant, (variant) => variant === enVariant);

			await variantContext.setCulture('da-DK');

			const currentVariant = await observeValue(resolver.currentVariant, (variant) => variant === daVariant);
			expect(currentVariant).to.equal(daVariant);
		});

		it('mirrors the result of getCurrentVariant()', async () => {
			const enVariant = makeVariant({ culture: 'en-US' });
			resolver.setVariants([enVariant], undefined);

			const currentVariant = await observeValue(resolver.currentVariant, (variant) => variant !== undefined);
			expect(currentVariant).to.equal(resolver.getCurrentVariant());
		});
	});

	describe('state and isDraft', () => {
		it('falls back to NotCreated when there is no current variant', async () => {
			resolver.setVariants([makeVariant({ culture: 'da-DK', state: UmbDocumentVariantState.PUBLISHED })], undefined);

			const state = await observeValue(resolver.state, (value) => value !== UmbDocumentVariantState.PUBLISHED);
			expect(state).to.equal(UmbDocumentVariantState.NOT_CREATED);
			expect(await observeValue(resolver.isDraft, () => true)).to.equal(false);
		});

		it('reports isDraft true only when the current variant is a draft', async () => {
			resolver.setVariants([makeVariant({ culture: 'en-US', state: UmbDocumentVariantState.DRAFT })], undefined);

			const isDraft = await observeValue(resolver.isDraft, (value) => value === true);
			expect(isDraft).to.equal(true);
		});
	});
});
