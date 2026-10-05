import { UmbContentWorkspaceDataManager } from './content-data-manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import type { UmbContentDetailModel } from '../types.js';

@customElement('test-content-data-manager-host')
class UmbTestContentDataManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type TestVariant = UmbEntityVariantModel & { name: string };
type TestModel = UmbContentDetailModel<TestVariant>;

describe('UmbContentWorkspaceDataManager', () => {
	let manager: UmbContentWorkspaceDataManager<TestModel>;

	const variant = (culture: string | null, segment: string | null = null, name = 'name'): TestVariant =>
		({ culture, segment, name }) as TestVariant;

	const setup = (variants: Array<TestVariant>) => {
		manager = new UmbContentWorkspaceDataManager<TestModel>(new UmbTestContentDataManagerHostElement(), variant(null));
		manager.setVariesByCulture(true);
		const data = { values: [], variants } as unknown as TestModel;
		manager.setPersisted(data);
		manager.setCurrent(data);
	};

	it('places a new variant by culture rather than at the start or end', () => {
		setup([variant('da-dk'), variant('zz-zz')]);

		manager.updateVariantData(new UmbVariantId('en-us'));

		expect(manager.getCurrent()?.variants.map((x) => x.culture)).to.deep.equal(['da-dk', 'en-us', 'zz-zz']);
	});

	it('places a new variant that sorts last at the end', () => {
		setup([variant('da-dk'), variant('en-us')]);

		manager.updateVariantData(new UmbVariantId('zz-zz'));

		expect(manager.getCurrent()?.variants.map((x) => x.culture)).to.deep.equal(['da-dk', 'en-us', 'zz-zz']);
	});

	it('keeps variants sorted when updating an existing one', () => {
		setup([variant('en-us'), variant('da-dk')]);

		manager.updateVariantData(new UmbVariantId('da-dk'), { name: 'changed' });

		const variants = manager.getCurrent()!.variants;
		expect(variants.map((x) => x.culture)).to.deep.equal(['da-dk', 'en-us']);
		expect(variants[0].name).to.equal('changed');
	});
});
