import { UmbLocalizeElement } from './localize.element.js';
import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbLocalizationRegistry } from './registry/localization.registry.js';

const english = {
	type: 'localization',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en',
		localizations: {
			general: {
				close: 'Close',
				logout: 'Log out',
				numUsersSelected: (count: number) => {
					if (count === 0) return 'No users selected';
					if (count === 1) return 'One user selected';
					return `${count} users selected`;
				},
				moreThanOneArgument: (arg1: string, arg2: string) => {
					return `${arg1} ${arg2}`;
				},
			},
		},
	},
};

const danish = {
	type: 'localization',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da',
		localizations: {
			general: {
				close: 'Luk',
			},
		},
	},
};

describe('umb-localize', () => {
	let element: UmbLocalizeElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-localize>Fallback value</umb-localize>`);
	});

	it('should be defined', () => {
		expect(element).to.be.instanceOf(UmbLocalizeElement);
	});

	describe('localization', () => {
		umbExtensionsRegistry.register(english);
		umbExtensionsRegistry.register(danish);

		beforeEach(async () => {
			umbLocalizationRegistry.loadLanguage(english.meta.culture);
			element = await fixture(html`<umb-localize key="general_close">Fallback value</umb-localize>`);
		});

		it('should have a localize controller', () => {
			expect(element.localize).to.be.instanceOf(UmbLocalizationController);
		});

		it('should localize a key', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');
		});

		it('should localize a key with arguments', async () => {
			element.key = 'general_numUsersSelected';
			element.args = [0];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('No users selected');

			element.args = [1];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('One user selected');

			element.args = [2];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('2 users selected');
		});

		it('should localize a key with multiple arguments', async () => {
			element.key = 'general_moreThanOneArgument';
			element.args = ['Hello', 'World'];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Hello World');
		});

		it('should localize a key with args as an attribute', async () => {
			element.key = 'general_moreThanOneArgument';
			element.setAttribute('args', '["Hello","World"]');
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Hello World');
		});

		it('should change the value if a new key is set', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			element.key = 'general_logout';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Log out');
		});

		it('should change the value if the language is changed', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			umbLocalizationRegistry.loadLanguage(danish.meta.culture);
			await aTimeout(0);
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Luk');
		});

		it('should use the slot if translation is not found', async () => {
			element.key = 'non-existing-key';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('<slot></slot>');
		});

		it('should toggle a data attribute', async () => {
			element.key = 'non-existing-key';
			await elementUpdated(element);

			expect(element.getAttribute('data-localize-missing')).to.equal('non-existing-key');

			element.key = 'general_close';
			await elementUpdated(element);

			expect(element.hasAttribute('data-localize-missing')).to.equal(false);
		});

		it('should use the key if debug is enabled and translation is not found', async () => {
			element.key = 'non-existing-key';
			element.debug = true;
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('non-existing-key');
		});
	});
});
