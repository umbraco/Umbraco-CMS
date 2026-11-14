import { UmbElementPropertyDatasetContext } from './element-property-dataset.context.js';
import type { UmbElementPropertyDataOwner } from './element-property-data-owner.interface.js';
import type { UmbElementDetailModel } from '../types.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { of, type Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

@customElement('umb-test-element-property-dataset-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestElementPropertyDatasetHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestElementPropertyDatasetContext extends UmbElementPropertyDatasetContext {
	name = of('Test');
	culture = of(this.getVariantId().culture);
	segment = of(this.getVariantId().segment);
	getName() {
		return 'Test';
	}
}

const property = (alias: string, variesByCulture: boolean): UmbPropertyTypeModel =>
	({ alias, variesByCulture, variesBySegment: false }) as UmbPropertyTypeModel;

const createDataOwner = (
	ownerVariesByCulture: boolean,
	properties: Array<UmbPropertyTypeModel>,
	values: UmbElementDetailModel['values'],
) =>
	({
		structure: {
			contentTypeProperties: of(properties),
			variesByCulture: of(ownerVariesByCulture),
			variesBySegment: of(false),
		},
		values: of(values),
		getValues: () => values,
		isLoaded: () => Promise.resolve(),
		readOnlyGuard: { isPermittedForVariant: () => of(false) },
	}) as unknown as UmbElementPropertyDataOwner<UmbElementDetailModel>;

const firstValue = <T>(observable: Observable<T> | undefined) =>
	new Promise<T>((resolve) => {
		const subscription = observable?.subscribe((value) => {
			resolve(value);
			queueMicrotask(() => subscription?.unsubscribe());
		});
	});

describe('UmbElementPropertyDatasetContext', () => {
	let host: UmbControllerHostElement;
	const english = new UmbVariantId('en-US', null);

	beforeEach(async () => {
		host = await fixture(html`<umb-test-element-property-dataset-host></umb-test-element-property-dataset-host>`);
	});

	describe('property variant id', () => {
		it('does not vary a property by culture when its owner content type is invariant', async () => {
			const dataOwner = createDataOwner(false, [property('composed', true)], []);
			const context = new UmbTestElementPropertyDatasetContext(host, dataOwner, english);

			const variantId = await firstValue(await context.propertyVariantId('composed'));

			expect(variantId?.culture).to.be.null;
		});

		it('varies a property by culture when both it and its owner content type vary', async () => {
			const dataOwner = createDataOwner(true, [property('composed', true)], []);
			const context = new UmbTestElementPropertyDatasetContext(host, dataOwner, english);

			const variantId = await firstValue(await context.propertyVariantId('composed'));

			expect(variantId?.culture).to.equal('en-US');
		});

		it('does not vary an invariant property in a varying owner content type', async () => {
			const dataOwner = createDataOwner(true, [property('own', false)], []);
			const context = new UmbTestElementPropertyDatasetContext(host, dataOwner, english);

			const variantId = await firstValue(await context.propertyVariantId('own'));

			expect(variantId?.culture).to.be.null;
		});
	});

	describe('property value', () => {
		it('reads the invariant value of a culture-varying property when its owner content type is invariant', async () => {
			const dataOwner = createDataOwner(
				false,
				[property('composed', true)],
				[{ alias: 'composed', culture: null, segment: null, value: 'stored', editorAlias: 'Umbraco.TextBox' }],
			);
			const context = new UmbTestElementPropertyDatasetContext(host, dataOwner, english);

			const value = await firstValue(await context.propertyValueByAlias('composed'));

			expect(value).to.equal('stored');
		});
	});
});
