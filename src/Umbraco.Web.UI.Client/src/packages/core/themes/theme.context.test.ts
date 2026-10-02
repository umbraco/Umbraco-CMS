import { UmbThemeContext } from './theme.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';
const TEST_THEME_ALIAS = 'Umb.Test.Theme.Custom';
const TEST_THEME_NO_CSS_ALIAS = 'Umb.Test.Theme.NoCss';

@customElement('umb-test-theme-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbThemeContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbThemeContext | undefined;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
	});

	afterEach(() => {
		context?.destroy();
		context = undefined;
		umbExtensionsRegistry.unregister(TEST_THEME_ALIAS);
		umbExtensionsRegistry.unregister(TEST_THEME_NO_CSS_ALIAS);
		localStorage.removeItem(LOCAL_STORAGE_KEY);
		document.head.querySelectorAll('link[href="/test-theme.css"]').forEach((link) => link.remove());
		hostElement.remove();
	});

	it('keeps the stored theme when the theme is registered after the context is created', () => {
		localStorage.setItem(LOCAL_STORAGE_KEY, TEST_THEME_ALIAS);
		context = new UmbThemeContext(hostElement);

		expect(localStorage.getItem(LOCAL_STORAGE_KEY)).to.equal(TEST_THEME_ALIAS);

		umbExtensionsRegistry.register({
			type: 'theme',
			alias: TEST_THEME_ALIAS,
			name: 'Test Theme',
			css: '/test-theme.css',
		});

		expect(localStorage.getItem(LOCAL_STORAGE_KEY)).to.equal(TEST_THEME_ALIAS);
		expect(document.head.querySelector('link[href="/test-theme.css"]')).to.not.be.null;
	});

	it('forgets the stored theme when the registered theme has no CSS', () => {
		umbExtensionsRegistry.register({
			type: 'theme',
			alias: TEST_THEME_NO_CSS_ALIAS,
			name: 'Test Theme Without CSS',
		});
		localStorage.setItem(LOCAL_STORAGE_KEY, TEST_THEME_NO_CSS_ALIAS);
		context = new UmbThemeContext(hostElement);

		expect(localStorage.getItem(LOCAL_STORAGE_KEY)).to.be.null;
	});
});
