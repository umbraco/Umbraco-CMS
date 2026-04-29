import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { TEST_MANIFESTS, UmbTestDocumentWorkspaceHostElement } from './document-workspace-context.test-utils.js';

const INVARIANT_DOCUMENT_ID = 'variant-documents-invariant-document-id';
const VARIANT_DOCUMENT_ID = 'variant-documents-variant-document-id';

describe('UmbDocumentWorkspaceContext', () => {
	let hostElement: UmbTestDocumentWorkspaceHostElement;
	let context: UmbDocumentWorkspaceContext;

	before(() => {
		umbExtensionsRegistry.registerMany(TEST_MANIFESTS);
	});

	after(() => {
		umbExtensionsRegistry.unregisterMany(TEST_MANIFESTS.map((m) => m.alias));
	});

	beforeEach(async () => {
		await useMockSet('documents');
		hostElement = new UmbTestDocumentWorkspaceHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
		context = new UmbDocumentWorkspaceContext(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('getPropertyValue', () => {
		describe('invariant document', () => {
			beforeEach(async () => {
				await context.load(INVARIANT_DOCUMENT_ID);
			});

			it('returns the invariant property value', () => {
				expect(context.getPropertyValue('text')).to.equal('This is the invariant text value.');
			});

			it('returns undefined for an unknown alias', () => {
				expect(context.getPropertyValue('nonExistent')).to.be.undefined;
			});
		});

		describe('variant document', () => {
			beforeEach(async () => {
				await context.load(VARIANT_DOCUMENT_ID);
			});

			it('returns the invariant property value without a variantId', () => {
				expect(context.getPropertyValue('text')).to.equal('This invariant text is shared across all cultures.');
			});

			it('returns the en-US variant property value', () => {
				const variantId = UmbVariantId.Create({ culture: 'en-US', segment: null });
				expect(context.getPropertyValue('variantText', variantId)).to.equal('This is the English variant text.');
			});

			it('returns the da variant property value', () => {
				const variantId = UmbVariantId.Create({ culture: 'da', segment: null });
				expect(context.getPropertyValue('variantText', variantId)).to.equal('Dette er den danske varianttekst.');
			});

			it('returns undefined for a culture-variant property when called without a variantId', () => {
				expect(context.getPropertyValue('variantText')).to.be.undefined;
			});

			it('returns undefined for an unknown alias', () => {
				expect(context.getPropertyValue('nonExistent')).to.be.undefined;
			});
		});
	});

	describe('setPropertyValue', () => {
		describe('invariant document', () => {
			beforeEach(async () => {
				await context.load(INVARIANT_DOCUMENT_ID);
			});

			it('updates the invariant property value', async () => {
				await context.setPropertyValue('text', 'Updated text value');
				expect(context.getPropertyValue('text')).to.equal('Updated text value');
			});

			it('throws for an unknown property alias', async () => {
				let error: Error | undefined;
				try {
					await context.setPropertyValue('nonExistent', 'some value');
				} catch (e) {
					error = e as Error;
				}
				expect(error?.message).to.equal('Property alias "nonExistent" not found.');
			});

			it('leaves the values observable functional after throwing for an unknown alias', async () => {
				try {
					await context.setPropertyValue('nonExistent', 'value');
				} catch (e) {
					// expected throw
				}
				const emissions: Array<unknown> = [];
				const sub = context.values.subscribe((v) => emissions.push(v));
				await context.setPropertyValue('text', 'probe after error');
				expect(emissions.length).to.be.greaterThan(1);
				sub.unsubscribe();
			});
		});

		describe('variant document', () => {
			beforeEach(async () => {
				await context.load(VARIANT_DOCUMENT_ID);
			});

			it('updates the en-US variant property value', async () => {
				const variantId = UmbVariantId.Create({ culture: 'en-US', segment: null });
				await context.setPropertyValue('variantText', 'Updated en-US text', variantId);
				expect(context.getPropertyValue('variantText', variantId)).to.equal('Updated en-US text');
			});

			it('does not affect other culture variants when updating one', async () => {
				const enUs = UmbVariantId.Create({ culture: 'en-US', segment: null });
				const da = UmbVariantId.Create({ culture: 'da', segment: null });
				await context.setPropertyValue('variantText', 'Updated en-US text', enUs);
				expect(context.getPropertyValue('variantText', da)).to.equal('Dette er den danske varianttekst.');
			});

			it('updates the invariant property value without a variantId', async () => {
				await context.setPropertyValue('text', 'Updated shared text');
				expect(context.getPropertyValue('text')).to.equal('Updated shared text');
			});

			it('does not create a phantom invariant entry when setting a culture-variant property without a variantId', async () => {
				try {
					await context.setPropertyValue('variantText', 'phantom value');
				} catch (e) {
					// expected: culture-variant property requires a variantId
				}
				const values = context.getValues();
				expect(values).to.be.an('array').with.lengthOf(4);
			});
		});
	});

	describe('getValues', () => {
		it('returns undefined when no data is loaded', () => {
			expect(context.getValues()).to.be.undefined;
		});

		it('returns all values after loading an invariant document', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			const values = context.getValues();
			expect(values).to.be.an('array').with.lengthOf(1);
			expect(values![0].alias).to.equal('text');
		});

		it('returns all values across all variants after loading a variant document', async () => {
			await context.load(VARIANT_DOCUMENT_ID);
			const values = context.getValues();
			expect(values).to.be.an('array').with.lengthOf(4);
		});

		it('reflects updates after setPropertyValue', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			await context.setPropertyValue('text', 'Updated');
			const values = context.getValues();
			expect(values?.find((v) => v.alias === 'text')?.value).to.equal('Updated');
		});
	});

	describe('values observable', () => {
		it('emits the values array on subscribe after load', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			let emitted: unknown;
			const sub = context.values.subscribe((v) => (emitted = v));
			expect(emitted).to.be.an('array').with.lengthOf(1);
			sub.unsubscribe();
		});

		it('emits updated values after setPropertyValue', async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
			const emissions: unknown[] = [];
			const sub = context.values.subscribe((v) => emissions.push(v));

			await context.setPropertyValue('text', 'Changed value');

			const last = emissions.at(-1) as Array<{ alias: string; value: unknown }>;
			expect(last?.find((v) => v.alias === 'text')?.value).to.equal('Changed value');
			sub.unsubscribe();
		});
	});

	describe('propertyValueByAlias', () => {
		describe('before load', () => {
			it('returns an observable that emits undefined when no document is loaded', async () => {
				const obs = await context.propertyValueByAlias<string>('text');
				let emitted: string | undefined = 'sentinel';
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.be.undefined;
				sub.unsubscribe();
			});
		});

		describe('invariant document', () => {
			beforeEach(async () => {
				await context.load(INVARIANT_DOCUMENT_ID);
			});

			it('returns an observable that emits the current value', async () => {
				const obs = await context.propertyValueByAlias<string>('text');
				let emitted: string | undefined;
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.equal('This is the invariant text value.');
				sub.unsubscribe();
			});

			it('returns an observable that emits undefined for an unknown alias', async () => {
				const obs = await context.propertyValueByAlias<string>('nonExistent');
				let emitted: string | undefined = 'sentinel';
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.be.undefined;
				sub.unsubscribe();
			});

			it('emits updated value after setPropertyValue', async () => {
				const obs = await context.propertyValueByAlias<string>('text');
				const emissions: Array<string | undefined> = [];
				const sub = obs!.subscribe((v) => emissions.push(v));

				await context.setPropertyValue('text', 'Live update');

				expect(emissions.at(-1)).to.equal('Live update');
				sub.unsubscribe();
			});
		});

		describe('variant document', () => {
			beforeEach(async () => {
				await context.load(VARIANT_DOCUMENT_ID);
			});

			it('returns an observable filtered by the en-US variantId', async () => {
				const variantId = UmbVariantId.Create({ culture: 'en-US', segment: null });
				const obs = await context.propertyValueByAlias<string>('variantText', variantId);
				let emitted: string | undefined;
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.equal('This is the English variant text.');
				sub.unsubscribe();
			});

			it('returns an observable filtered by the da variantId', async () => {
				const variantId = UmbVariantId.Create({ culture: 'da', segment: null });
				const obs = await context.propertyValueByAlias<string>('variantText', variantId);
				let emitted: string | undefined;
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.equal('Dette er den danske varianttekst.');
				sub.unsubscribe();
			});

			it('returns undefined for a variant property when no variantId is given (invariant-only fallback)', async () => {
				const obs = await context.propertyValueByAlias<string>('variantText');
				let emitted: string | undefined = 'sentinel';
				const sub = obs!.subscribe((v) => (emitted = v));
				expect(emitted).to.be.undefined;
				sub.unsubscribe();
			});
		});
	});

	describe('initiatePropertyValueChange / finishPropertyValueChange', () => {
		beforeEach(async () => {
			await context.load(INVARIANT_DOCUMENT_ID);
		});

		it('suppresses values emissions while changes are initiated and flushes on finish', async () => {
			const emissions: unknown[] = [];
			const sub = context.values.subscribe((v) => emissions.push(v));
			const countAtStart = emissions.length;

			context.initiatePropertyValueChange();
			await context.setPropertyValue('text', 'Batched value');

			expect(emissions.length).to.equal(countAtStart);

			context.finishPropertyValueChange();

			expect(emissions.length).to.equal(countAtStart + 1);
			expect(context.getPropertyValue('text')).to.equal('Batched value');
			sub.unsubscribe();
		});

		it('emits once after finishPropertyValueChange', async () => {
			const emissions: unknown[] = [];
			const sub = context.values.subscribe((v) => emissions.push(v));
			const countAtStart = emissions.length;

			context.initiatePropertyValueChange();
			await context.setPropertyValue('text', 'Value 1');
			await context.setPropertyValue('text', 'Value 2');
			context.finishPropertyValueChange();

			expect(emissions.length).to.equal(countAtStart + 1);

			const last = emissions.at(-1) as Array<{ alias: string; value: unknown }>;
			expect(last?.find((v) => v.alias === 'text')?.value).to.equal('Value 2');
			sub.unsubscribe();
		});

		it('supports nested initiate/finish pairs', async () => {
			const emissions: unknown[] = [];
			const sub = context.values.subscribe((v) => emissions.push(v));
			const countAtStart = emissions.length;

			context.initiatePropertyValueChange();
			context.initiatePropertyValueChange();
			await context.setPropertyValue('text', 'Nested value');

			context.finishPropertyValueChange();
			expect(emissions.length).to.equal(countAtStart); // still locked

			context.finishPropertyValueChange();
			expect(emissions.length).to.equal(countAtStart + 1); // now emits
			sub.unsubscribe();
		});
	});
});
