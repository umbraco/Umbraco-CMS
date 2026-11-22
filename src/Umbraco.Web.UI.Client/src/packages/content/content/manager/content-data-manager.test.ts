import { UmbContentWorkspaceDataManager } from './content-data-manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import type { UmbContentDetailModel, UmbEntryValueModel } from '../types.js';

@customElement('test-content-data-manager-host')
class UmbTestContentDataManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type TestVariant = UmbEntityVariantModel & { name: string };
type TestModel = UmbContentDetailModel<TestVariant>;

describe('UmbContentWorkspaceDataManager', () => {
	let manager: UmbContentWorkspaceDataManager<TestModel>;

	const variant = (culture: string | null, name = 'name'): TestVariant => ({ culture, name }) as TestVariant;

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

	describe('getChangedVariants', () => {
		const value = (culture: string | null, value: unknown): UmbEntryValueModel => ({
			editorAlias: 'Umbraco.TrueFalse',
			alias: 'toggle',
			culture,
			segment: null,
			value,
		});

		const setupWithData = (variants: Array<TestVariant>, values: Array<UmbEntryValueModel> = []) => {
			manager = new UmbContentWorkspaceDataManager<TestModel>(
				new UmbTestContentDataManagerHostElement(),
				variant(null, ''),
			);
			manager.setVariesByCulture(true);
			const data = { values, variants } as unknown as TestModel;
			manager.setPersisted(data);
			manager.setCurrent(data);
		};

		const changedCultures = () => manager.getChangedVariants().map((x) => x.culture);

		it('reports nothing when the current data equals the persisted data', () => {
			setupWithData([variant('en-us')], [value('en-us', true)]);

			expect(changedCultures()).to.be.empty;
		});

		it('treats a variant without a name and no persisted counterpart as non-existing', () => {
			setupWithData([variant('en-us')]);

			manager.ensureVariantData(new UmbVariantId('da-dk'));

			expect(manager.getCurrent()?.variants.map((x) => x.culture)).to.include('da-dk');
			expect(changedCultures()).to.be.empty;
		});

		it('reports a named variant that has no persisted counterpart', () => {
			setupWithData([variant('en-us')]);

			manager.updateVariantData(new UmbVariantId('da-dk'), { name: 'new' });

			expect(changedCultures()).to.deep.equal(['da-dk']);
		});

		it('reports a variant whose name differs from the persisted variant', () => {
			setupWithData([variant('en-us'), variant('da-dk')]);

			manager.updateVariantData(new UmbVariantId('da-dk'), { name: 'changed' });

			expect(changedCultures()).to.deep.equal(['da-dk']);
		});

		it('reports the variant of a value that differs from the persisted value', () => {
			setupWithData([variant('en-us'), variant('da-dk')], [value('en-us', true), value('da-dk', true)]);

			manager.updateCurrent({ values: [value('en-us', true), value('da-dk', false)] } as Partial<TestModel>);

			expect(changedCultures()).to.deep.equal(['da-dk']);
		});

		it('reports the variant of a value that has no persisted counterpart', () => {
			setupWithData([variant('en-us')], [value('en-us', true)]);

			manager.updateCurrent({ values: [value('en-us', true), value('da-dk', true)] } as Partial<TestModel>);

			expect(changedCultures()).to.deep.equal(['da-dk']);
		});
	});
});
