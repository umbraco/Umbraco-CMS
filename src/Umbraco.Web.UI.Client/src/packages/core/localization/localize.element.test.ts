import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { UmbLocalizeElement } from './localize.element.js';
import { UMB_LOCALIZATION_CONTEXT, UmbLocalizationContext } from '@umbraco-cms/backoffice/localization-api';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

import '@umbraco-cms/backoffice/context-api';

const english = {
	type: 'translations',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en',
		translations: {
			general: {
				close: 'Close',
				logout: 'Log out',
			},
		},
	},
};

const danish = {
	type: 'translations',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da',
		translations: {
			general: {
				close: 'Luk',
			},
		},
	},
};

describe('umb-localize', () => {
	let element: UmbLocalizeElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-localize></umb-localize>`);
	});

	it('should be defined', () => {
		expect(element).to.be.instanceOf(UmbLocalizeElement);
	});

	describe('localization', () => {
		let hostElement: HTMLElement;
		let extensionRegistry: UmbExtensionRegistry<never>;
		let context: UmbLocalizationContext;

		beforeEach(async () => {
			extensionRegistry = new UmbExtensionRegistry();
			extensionRegistry.register(english);
			extensionRegistry.register(danish);
			context = new UmbLocalizationContext(extensionRegistry);
			context.setLanguage(english.meta.culture, english.meta.culture);
			hostElement = await fixture(
				html`<umb-context-provider .key=${UMB_LOCALIZATION_CONTEXT} .value=${context}>
					<umb-localize key="general_close"></umb-localize>
				</umb-context-provider>`
			);
			element = hostElement.querySelector('umb-localize') as UmbLocalizeElement;
		});

		it('should localize a key', async () => {
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Close');
		});

		it('should change the value if a new key is set', async () => {
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			element.key = 'general_logout';
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Log out');
		});

		it('should change the value if the language is changed', async () => {
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			context.setLanguage(danish.meta.culture);
			await elementUpdated(element);

			await aTimeout(0);

			expect(element.shadowRoot?.innerHTML).to.contain('Luk');
		});
	});
});
