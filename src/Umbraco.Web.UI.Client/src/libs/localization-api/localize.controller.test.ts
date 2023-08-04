import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { DefaultTranslationSet, TranslationSet, registerTranslation, translations } from './manager.js';
import { UmbLocalizeController } from './localize.controller.js';
import { LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-localize-controller-host')
class UmbLocalizeControllerHostElement extends UmbElementMixin(LitElement) {
	@property() lang = 'en-us';
}

interface TestTranslation extends TranslationSet {
	close: string;
	logout: string;
	withInlineToken: any;
	withInlineTokenLegacy: any;
	notOnRegional: string;
	numUsersSelected: (count: number) => string;
}

//#region Translations
const english: TestTranslation = {
	$code: 'en-us',
	$dir: 'ltr',
	close: 'Close',
	logout: 'Log out',
	withInlineToken: '{0} {1}',
	withInlineTokenLegacy: '%0% %1%',
	notOnRegional: 'Not on regional',
	numUsersSelected: (count: number) => {
		if (count === 0) return 'No users selected';
		if (count === 1) return 'One user selected';
		return `${count} users selected`;
	},
};

const englishOverride: DefaultTranslationSet = {
	$code: 'en-us',
	$dir: 'ltr',
	close: 'Close 2',
};

const danish: DefaultTranslationSet = {
	$code: 'da',
	$dir: 'ltr',
	close: 'Luk',
	notOnRegional: 'Not on regional',
};

const danishRegional: DefaultTranslationSet = {
	$code: 'da-dk',
	$dir: 'ltr',
	close: 'Luk',
};
//#endregion

describe('UmbLocalizeController', () => {
	let controller: UmbLocalizeController<TestTranslation>;

	beforeEach(async () => {
		registerTranslation(english, danish, danishRegional);
		document.documentElement.lang = english.$code;
		document.documentElement.dir = english.$dir;
		await aTimeout(0);
		const host = {
			getHostElement: () => document.createElement('div'),
			addController: () => {},
			removeController: () => {},
			hasController: () => false,
			getControllers: () => [],
			removeControllerByAlias: () => {},
		} satisfies UmbControllerHost;
		controller = new UmbLocalizeController(host);
	});

	afterEach(() => {
		controller.destroy();
		translations.clear();
	});

	it('should have a default language', () => {
		expect(controller.lang()).to.equal(english.$code);
	});

	it('should update the language when the language changes', async () => {
		document.documentElement.lang = danishRegional.$code;
		await aTimeout(0);
		expect(controller.lang()).to.equal(danishRegional.$code);
	});

	it('should have a default dir', () => {
		expect(controller.dir()).to.equal(english.$dir);
	});

	it('should update the dir when the dir changes', async () => {
		document.documentElement.dir = 'rtl';
		await aTimeout(0);
		expect(controller.dir()).to.equal('rtl');
	});

	describe('term', () => {
		it('should return a term', async () => {
			expect(controller.term('close')).to.equal('Close');
		});

		it('should update the term when the language changes', async () => {
			// Change language
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.term('close')).to.equal('Luk');
		});

		it('should provide a secondary term when the term is not found on the regional language', async () => {
			// Load Danish
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.term('notOnRegional')).to.equal('Not on regional');
		});

		it('should provide a fallback term from the fallback language when the term is not found on primary or secondary', async () => {
			// Load Danish
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.term('close')).to.equal('Luk'); // Primary
			expect(controller.term('logout')).to.equal('Log out'); // Fallback
		});

		it('should override a term if new translation is registered', () => {
			// Let the registry load the new extension
			registerTranslation(englishOverride);

			expect(controller.term('close')).to.equal('Close 2');
		});

		it('should return a term with a custom format', async () => {
			expect(controller.term('numUsersSelected', 0)).to.equal('No users selected');
			expect(controller.term('numUsersSelected', 1)).to.equal('One user selected');
			expect(controller.term('numUsersSelected', 2)).to.equal('2 users selected');
		});

		it('should return a term with a custom format with inline tokens', async () => {
			expect(controller.term('withInlineToken', 'Hello', 'World')).to.equal('Hello World');
			expect(controller.term('withInlineTokenLegacy', 'Hello', 'World')).to.equal('Hello World');
		});

		it('should return a term with no tokens even though they are provided', async () => {
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			expect((controller.term as any)('logout', 'Hello', 'World')).to.equal('Log out');
		});
	});

	describe('date', () => {
		it('should return a date', async () => {
			expect(controller.date(new Date(2020, 0, 1))).to.equal('1/1/2020');
		});

		it('should accept a string input', async () => {
			expect(controller.date('2020-01-01')).to.equal('1/1/2020');
		});

		it('should update the date when the language changes', async () => {
			expect(controller.date(new Date(2020, 11, 31))).to.equal('12/31/2020');

			// Switch browser to Danish
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.date(new Date(2020, 11, 31))).to.equal('31.12.2020');
		});

		it('should return a date with a custom format', () => {
			expect(controller.date(new Date(2020, 11, 31), { month: 'long', day: '2-digit', year: 'numeric' })).to.equal(
				'December 31, 2020'
			);
		});
	});

	describe('number', () => {
		it('should return a number', () => {
			expect(controller.number(123456.789)).to.equal('123,456.789');
		});

		it('should accept a string input', () => {
			expect(controller.number('123456.789')).to.equal('123,456.789');
		});

		it('should update the number when the language changes', async () => {
			// Switch browser to Danish
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.number(123456.789)).to.equal('123.456,789');
		});

		it('should return a number with a custom format', () => {
			expect(controller.number(123456.789, { minimumFractionDigits: 2, maximumFractionDigits: 2 })).to.equal(
				'123,456.79'
			);
		});
	});

	describe('relative time', () => {
		it('should return a relative time', () => {
			expect(controller.relativeTime(2, 'days')).to.equal('in 2 days');
		});

		it('should update the relative time when the language changes', async () => {
			// Switch browser to Danish
			document.documentElement.lang = danishRegional.$code;
			await aTimeout(0);

			expect(controller.relativeTime(2, 'days')).to.equal('om 2 dage');
		});
	});

	describe('host element', () => {
		let element: UmbLocalizeControllerHostElement;

		beforeEach(async () => {
			element = await fixture(html`<umb-localize-controller-host></umb-localize-controller-host>`);
		});

		it('should have a localize controller', () => {
			expect(element.localize).to.be.instanceOf(UmbLocalizeController);
		});

		it('should update the term when the language changes', async () => {
			expect(element.localize.term('close')).to.equal('Close');

			// Switch browser to Danish
			element.lang = danishRegional.$code;

			await elementUpdated(element);
			expect(element.localize.term('close')).to.equal('Luk');
		});
	});
});
