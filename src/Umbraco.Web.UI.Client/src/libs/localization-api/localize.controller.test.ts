import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { UmbLocalizeController } from './localize.controller.js';
import { UmbTranslationRegistry } from './registry/translation.registry.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-localize-controller-host')
class UmbLocalizeControllerHostElement extends UmbLitElement {
	@property()
	lang = 'en-us';
}

//#region Translations
const english = {
	type: 'translations',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		translations: {
			general: {
				close: 'Close',
				logout: 'Log out',
				numUsersSelected: (count: number) => {
					if (count === 0) return 'No users selected';
					if (count === 1) return 'One user selected';
					return `${count} users selected`;
				},
			},
		},
	},
};

const englishOverride = {
	type: 'translations',
	alias: 'test.en.override',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		translations: {
			general: {
				close: 'Close 2',
			},
		},
	},
};

const danish = {
	type: 'translations',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da-dk',
		translations: {
			general: {
				close: 'Luk',
			},
		},
	},
};
//#endregion

describe('UmbLocalizeController', () => {
	const registry = new UmbTranslationRegistry(umbExtensionsRegistry);
	umbExtensionsRegistry.register(english);
	umbExtensionsRegistry.register(danish);

	let element: UmbLocalizeControllerHostElement;

	beforeEach(async () => {
		registry.loadLanguage(english.meta.culture);
		element = await fixture(html`<umb-localize-controller-host></umb-localize-controller-host>`);
	});

	it('should have a localize controller', () => {
		expect(element.localize).to.be.instanceOf(UmbLocalizeController);
	});

	describe('term', () => {
		it('should return a term', async () => {
			expect(element.localize.term('general_close')).to.equal('Close');
		});

		it('should update the term when the language changes', async () => {
			expect(element.localize.term('general_close')).to.equal('Close');
			// Load Danish
			registry.loadLanguage(danish.meta.culture);
			// Switch browser to Danish
			element.lang = danish.meta.culture;

			await elementUpdated(element);
			expect(element.localize.term('general_close')).to.equal('Luk');
		});

		it('should update the term when the dir changes', async () => {
			expect(element.localize.term('general_close')).to.equal('Close');
			element.dir = 'rtl';
			await elementUpdated(element);
			expect(element.localize.term('general_close')).to.equal('Close');
		});

		it('should provide a fallback term when the term is not found', async () => {
			// Load Danish
			registry.loadLanguage(danish.meta.culture);
			// Switch browser to Danish
			element.lang = danish.meta.culture;
			await elementUpdated(element);
			expect(element.localize.term('general_close')).to.equal('Luk');
			expect(element.localize.term('general_logout')).to.equal('Log out');
		});

		it('should override a term if new extension is registered', async () => {
			umbExtensionsRegistry.register(englishOverride);
			// Let the registry load the new extension
			await aTimeout(0);
			await elementUpdated(element);
			expect(element.localize.term('general_close')).to.equal('Close 2');
		});

		it('should return a term with a custom format', async () => {
			expect(element.localize.term('general_numUsersSelected', 0)).to.equal('No users selected');
			expect(element.localize.term('general_numUsersSelected', 1)).to.equal('One user selected');
			expect(element.localize.term('general_numUsersSelected', 2)).to.equal('2 users selected');
		});
	});

	describe('date', () => {
		it('should return a date', async () => {
			expect(element.localize.date(new Date(2020, 0, 1))).to.equal('1/1/2020');
		});

		it('should accept a string input', async () => {
			expect(element.localize.date('2020-01-01')).to.equal('1/1/2020');
		});

		it('should update the date when the language changes', async () => {
			expect(element.localize.date(new Date(2020, 11, 31))).to.equal('12/31/2020');

			// Switch browser to Danish
			element.lang = danish.meta.culture;

			await elementUpdated(element);
			expect(element.localize.date(new Date(2020, 11, 31))).to.equal('31.12.2020');
		});

		it('should update the date when the dir changes', async () => {
			expect(element.localize.date(new Date(2020, 11, 31))).to.equal('12/31/2020');
			element.dir = 'rtl';
			await elementUpdated(element);
			expect(element.localize.date(new Date(2020, 11, 31))).to.equal('12/31/2020');
		});

		it('should return a date with a custom format', async () => {
			expect(
				element.localize.date(new Date(2020, 11, 31), { month: 'long', day: '2-digit', year: 'numeric' })
			).to.equal('December 31, 2020');
		});
	});

	describe('number', () => {
		it('should return a number', async () => {
			expect(element.localize.number(123456.789)).to.equal('123,456.789');
		});

		it('should accept a string input', async () => {
			expect(element.localize.number('123456.789')).to.equal('123,456.789');
		});

		it('should update the number when the language changes', async () => {
			expect(element.localize.number(123456.789)).to.equal('123,456.789');

			// Switch browser to Danish
			element.lang = danish.meta.culture;

			await elementUpdated(element);
			expect(element.localize.number(123456.789)).to.equal('123.456,789');
		});

		it('should update the number when the dir changes', async () => {
			expect(element.localize.number(123456.789)).to.equal('123,456.789');
			element.dir = 'rtl';
			await elementUpdated(element);
			expect(element.localize.number(123456.789)).to.equal('123,456.789');
		});

		it('should return a number with a custom format', async () => {
			expect(element.localize.number(123456.789, { minimumFractionDigits: 2, maximumFractionDigits: 2 })).to.equal(
				'123,456.79'
			);
		});
	});

	describe('relative time', () => {
		it('should return a relative time', async () => {
			expect(element.localize.relativeTime(2, 'days')).to.equal('in 2 days');
		});

		it('should update the relative time when the language changes', async () => {
			expect(element.localize.relativeTime(2, 'days')).to.equal('in 2 days');

			// Switch browser to Danish
			element.lang = danish.meta.culture;

			await elementUpdated(element);
			expect(element.localize.relativeTime(2, 'days')).to.equal('om 2 dage');
		});
	});
});
