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
});
