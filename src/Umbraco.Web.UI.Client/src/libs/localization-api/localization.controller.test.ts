import type { UmbLocalizationSet, UmbLocalizationSetBase } from './localization.manager.js';
import { umbLocalizationManager } from './localization.manager.js';
import { UmbLocalizationController } from './localization.controller.js';
import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { LitElement, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const initialLanguage = 'en-us';

@customElement('umb-localize-controller-host')
class UmbLocalizeControllerHostElement extends UmbElementMixin(LitElement) {
	@property() override lang = initialLanguage;
}

@customElement('umb-localization-render-count')
class UmbLocalizationRenderCountElement extends UmbElementMixin(LitElement) {
	amountOfUpdates = 0;

	override requestUpdate() {
		super.requestUpdate();
		this.amountOfUpdates++;
	}

	override render() {
		return html`${this.localize.term('logout')}`;
	}
}

interface TestLocalization extends UmbLocalizationSetBase {
	close: string;
	logout: string;
	withInlineToken: any;
	withInlineTokenLegacy: any;
	notOnRegional: string;
	numUsersSelected: (count: number) => string;
}

//#region Localizations
const english: TestLocalization = {
	$code: 'en',
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

const englishOverride: UmbLocalizationSet = {
	$code: 'en-us',
	$dir: 'ltr',
	close: 'Close 2',
};

const englishOverrideLogout: UmbLocalizationSet = {
	$code: 'en-us',
	$dir: 'ltr',
	logout: 'Log out 2',
};

const danish: UmbLocalizationSet = {
	$code: 'da',
	$dir: 'ltr',
	close: 'Luk',
	notOnRegional: 'Not on regional',
};

const danishRegional: UmbLocalizationSet = {
	$code: 'da-dk',
	$dir: 'ltr',
	close: 'Luk',
};
//#endregion

describe('UmbLocalizeController', () => {
	let controller: UmbLocalizationController;

	beforeEach(async () => {
		umbLocalizationManager.registerManyLocalizations([english, danish, danishRegional]);
		document.documentElement.lang = initialLanguage;
		document.documentElement.dir = 'ltr';
		await aTimeout(0);
		const host = {
			getHostElement: () => document.createElement('div'),
			addUmbController: () => {},
			removeUmbController: () => {},
			hasUmbController: () => false,
			getUmbControllers: () => [],
			removeUmbControllerByAlias: () => {},
		} satisfies UmbControllerHost;
		controller = new UmbLocalizationController(host);
	});

	afterEach(() => {
		controller.destroy();
		umbLocalizationManager.localizations.clear();
	});

	it('should have a default language', () => {
		expect(controller.lang()).to.equal(initialLanguage);
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

		it('should override a term if new localization is registered', () => {
			// Let the registry load the new extension
			umbLocalizationManager.registerLocalization(englishOverride);

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

		it('should encode HTML entities', () => {
			expect(controller.term('withInlineToken', 'Hello', '<script>alert("XSS")</script>'), 'XSS detected').to.equal(
				'Hello &lt;script&gt;alert(&#34;XSS&#34;)&lt;/script&gt;',
			);
		});

		it('only reacts to changes of its own localization-keys', async () => {
			const element: UmbLocalizationRenderCountElement = await fixture(
				html`<umb-localization-render-count></umb-localization-render-count>`,
			);

			// Something triggers multiple updates initially, and it varies how many it is. So we wait for a timeout to ensure that we have a clean slate and then reset the counter:
			await aTimeout(20);
			element.amountOfUpdates = 0;

			expect(element.shadowRoot!.textContent).to.equal('Log out');

			// Let the registry load the new extension
			umbLocalizationManager.registerLocalization(englishOverride);

			// Wait three frames is safe:
			await new Promise((resolve) => requestAnimationFrame(resolve));
			await new Promise((resolve) => requestAnimationFrame(resolve));
			await new Promise((resolve) => requestAnimationFrame(resolve));

			// This should still be the same (cause it should not be affected as the change did not change our localization key)
			expect(element.amountOfUpdates).to.equal(0);
			expect(element.shadowRoot!.textContent).to.equal('Log out');

			// Let the registry load the new extension
			umbLocalizationManager.registerLocalization(englishOverrideLogout);

			// Wait three frames is safe:
			await new Promise((resolve) => requestAnimationFrame(resolve));
			await new Promise((resolve) => requestAnimationFrame(resolve));
			await new Promise((resolve) => requestAnimationFrame(resolve));

			// Now we should have gotten one update and the text should be different
			expect(element.amountOfUpdates).to.equal(1);
			expect(element.shadowRoot!.textContent).to.equal('Log out 2');
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
				'December 31, 2020',
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
				'123,456.79',
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

	describe('string', () => {
		it('should replace words prefixed with a # with translated value', async () => {
			const str = '#close';
			const str2 = '#logout #close';
			const str3 = '#logout #missing_translation_key #close';
			expect(controller.string(str)).to.equal('Close');
			expect(controller.string(str2)).to.equal('Log out Close');
			expect(controller.string(str3)).to.equal('Log out #missing_translation_key Close');
		});

		it('should return the word with a # if the word is not found', async () => {
			const str = '#missing_translation_key';
			expect(controller.string(str)).to.equal('#missing_translation_key');
		});

		it('should return an empty string if the input is not a string', async () => {
			expect(controller.string(123 as any)).to.equal('');
			expect(controller.string({} as any)).to.equal('');
			expect(controller.string(undefined)).to.equal('');
		});

		it('should return an empty string if the input is an empty string', async () => {
			expect(controller.string('')).to.equal('');
		});

		it('should return the input string if the input is not prefixed with a #', async () => {
			const str = 'close';
			expect(controller.string(str)).to.equal('close');
		});

		it('should replace tokens in each key with the provided args', async () => {
			const str = '#withInlineToken #withInlineTokenLegacy';
			expect(controller.string(str, 'value1', 'value2')).to.equal('value1 value2 value1 value2');
		});
	});

	describe('host element', () => {
		let element: UmbLocalizeControllerHostElement;

		beforeEach(async () => {
			element = await fixture(html`<umb-localize-controller-host></umb-localize-controller-host>`);
		});

		it('should have a localize controller', () => {
			expect(element.localize).to.be.instanceOf(UmbLocalizationController);
		});

		it('should update the term when the language changes', async () => {
			expect(element.localize.term('close')).to.equal('Close');

			// Switch browser to Danish
			element.lang = danishRegional.$code;

			await elementUpdated(element);
			expect(element.localize.term('close')).to.equal('Luk');
		});

		it('should update the string when the language changes', async () => {
			expect(element.localize.string('testing #close')).to.equal('testing Close');

			// Switch browser to Danish
			element.lang = danishRegional.$code;

			await elementUpdated(element);
			expect(element.localize.string('testing #close')).to.equal('testing Luk');
		});
	});
});
