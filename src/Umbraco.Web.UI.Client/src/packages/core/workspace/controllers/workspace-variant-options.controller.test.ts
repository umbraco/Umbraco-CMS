import { UmbWorkspaceVariantOptionsController } from './workspace-variant-options.controller.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_APP_LANGUAGE_CONTEXT, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbArrayState, UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

const createLanguage = (unique: string, isDefault = false): UmbLanguageDetailModel => ({
	entityType: 'language',
	unique,
	name: unique,
	isDefault,
	isMandatory: false,
	fallbackIsoCode: null,
});

const EN = createLanguage('en-US', true);
const DA = createLanguage('da');

class UmbAppLanguageContextStub extends UmbContextBase {
	readonly #languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	readonly languages = this.#languages.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_APP_LANGUAGE_CONTEXT);
	}

	setLanguages(languages: Array<UmbLanguageDetailModel>) {
		this.#languages.setValue(languages);
	}
}

@customElement('umb-test-variant-options-host')
class UmbTestVariantOptionsHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	languageContext!: UmbAppLanguageContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.languageContext = new UmbAppLanguageContextStub(this);
	}
}

async function settle() {
	await new Promise((resolve) => setTimeout(resolve, 0));
}

describe('UmbWorkspaceVariantOptionsController', () => {
	let host: UmbTestVariantOptionsHostElement;
	let controller: UmbWorkspaceVariantOptionsController<UmbEntityVariantModel>;
	let varyByCulture: UmbBasicState<boolean | undefined>;
	let varyBySegment: UmbBasicState<boolean | undefined>;
	let variants: UmbBasicState<Array<UmbEntityVariantModel>>;
	let latest: Array<UmbEntityVariantOptionModel> | undefined;
	let emissions: number;

	const uniques = () => latest?.map((option) => option.unique);

	beforeEach(async () => {
		host = await fixture(html`<umb-test-variant-options-host></umb-test-variant-options-host>`);
		varyByCulture = new UmbBasicState<boolean | undefined>(undefined);
		varyBySegment = new UmbBasicState<boolean | undefined>(undefined);
		variants = new UmbBasicState<Array<UmbEntityVariantModel>>([]);
		latest = undefined;
		emissions = 0;

		controller = new UmbWorkspaceVariantOptionsController(
			host,
			varyByCulture.asObservable(),
			varyBySegment.asObservable(),
			variants.asObservable(),
			host.languageContext.languages,
		);
		controller.observe(
			controller.variantOptions,
			(options) => {
				latest = options;
				emissions++;
			},
			null,
		);
	});

	afterEach(() => {
		controller.destroy();
		host.remove();
	});

	describe('while loading', () => {
		it('emits no options before the content type variation is known', async () => {
			host.languageContext.setLanguages([EN, DA]);
			await settle();

			expect(latest).to.be.undefined;
		});

		it('emits undefined while loading and the options once loaded', async () => {
			const seen: Array<unknown> = [];
			controller.observe(controller.variantOptions, (options) => seen.push(options), 'seen');
			await settle();
			expect(seen[0]).to.be.undefined;

			host.languageContext.setLanguages([EN, DA]);
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			expect(seen[seen.length - 1]).to.be.an('array');
		});
	});

	describe('core options', () => {
		beforeEach(() => {
			host.languageContext.setLanguages([EN, DA]);
		});

		it('provides a single invariant option, using the default language, when the content does not vary', async () => {
			varyByCulture.setValue(false);
			varyBySegment.setValue(false);
			await settle();

			expect(latest).to.have.lengthOf(1);
			expect(latest![0]).to.include({ culture: null, segment: null });
			expect(latest![0].language).to.equal(EN);
		});

		it('provides one option per language when the content varies by culture', async () => {
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da']);
			expect(latest!.find((option) => option.culture === 'da')?.language).to.equal(DA);
		});

		it('provides only the invariant option when the content varies by segment alone', async () => {
			varyByCulture.setValue(false);
			varyBySegment.setValue(true);
			await settle();

			expect(latest).to.have.lengthOf(1);
			expect(latest![0]).to.include({ culture: null, segment: null });
		});

		it('provides only culture options, without segment options, when varying by both', async () => {
			varyByCulture.setValue(true);
			varyBySegment.setValue(true);
			await settle();

			expect(latest!.every((option) => option.segment === null)).to.be.true;
			expect(uniques()).to.have.members(['en-US', 'da']);
		});

		it('connects each option to the variant of its culture', async () => {
			const daVariant = { culture: 'da', segment: null, name: 'Dansk' } as unknown as UmbEntityVariantModel;
			variants.setValue([daVariant]);
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			expect(latest!.find((option) => option.culture === 'da')?.variant).to.equal(daVariant);
			expect(latest!.find((option) => option.culture === 'en-US')?.variant).to.be.undefined;
		});

		it('follows a language that is added later', async () => {
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			host.languageContext.setLanguages([EN, DA, createLanguage('de')]);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da', 'de']);
		});

		it('drops options for a culture the content no longer varies by', async () => {
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			varyByCulture.setValue(false);
			await settle();

			expect(latest).to.have.lengthOf(1);
			expect(latest![0].culture).to.be.null;
		});
	});

	describe('additional options', () => {
		beforeEach(async () => {
			host.languageContext.setLanguages([EN, DA]);
			varyByCulture.setValue(true);
			varyBySegment.setValue(true);
			await settle();
		});

		it('leaves out an option whose language cannot be found', async () => {
			controller.add([
				{ culture: 'da', segment: 's1', unique: 'da_s:s1' },
				{ culture: 'xx', segment: 's1', unique: 'xx_s:s1' },
			]);
			await settle();

			expect(uniques()).to.include('da_s:s1');
			expect(uniques()).to.not.include('xx_s:s1');
		});

		it('adds options next to the core options, and gives them a language', async () => {
			controller.add([{ culture: 'da', segment: 's1', unique: 'da_s:s1' }]);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da', 'da_s:s1']);
			expect(latest!.find((option) => option.unique === 'da_s:s1')?.language).to.equal(DA);
		});

		it('removes options by unique, leaving the others', async () => {
			controller.add([
				{ culture: 'da', segment: 's1', unique: 'da_s:s1' },
				{ culture: 'en-US', segment: 's1', unique: 'en-US_s:s1' },
			]);
			controller.remove(['da_s:s1']);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da', 'en-US_s:s1']);
		});

		it('replaces options in a single emission', async () => {
			controller.add([{ culture: 'da', segment: 's1', unique: 'da_s:s1' }]);
			await settle();
			const before = emissions;

			controller.replace(['da_s:s1'], [{ culture: 'da', segment: 's2', unique: 'da_s:s2' }]);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da', 'da_s:s2']);
			expect(emissions - before).to.equal(1);
		});

		it('keeps added options when the core options are recomputed', async () => {
			controller.add([{ culture: 'da', segment: 's1', unique: 'da_s:s1' }]);
			variants.setValue([{ culture: 'da', segment: null, name: 'Dansk' } as unknown as UmbEntityVariantModel]);
			await settle();

			expect(uniques()).to.include('da_s:s1');
		});

		it('keeps core options when added options are removed', async () => {
			controller.add([{ culture: 'da', segment: 's1', unique: 'da_s:s1' }]);
			controller.remove(['da_s:s1']);
			await settle();

			expect(uniques()).to.have.members(['en-US', 'da']);
		});
	});

	describe('option filter', () => {
		it('only emits options the filter accepts', async () => {
			host.languageContext.setLanguages([EN, DA]);
			controller._internal_setOptionFilter((option) => option.culture !== 'da');
			controller.observe(controller.variantOptions, (options) => (latest = options), 'filtered');
			varyByCulture.setValue(true);
			varyBySegment.setValue(false);
			await settle();

			expect(uniques()).to.deep.equal(['en-US']);
		});
	});
});
