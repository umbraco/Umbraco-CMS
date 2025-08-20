import UmbTiptapUnderlineExtensionApi from './underline.tiptap-api.js';
import UmbTiptapLinkExtensionApi from './link.tiptap-api.js';
import UmbTiptapWordCountExtensionApi from './word-count.tiptap-api.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

// Test controller host element
@customElement('test-core-extension-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTiptapUnderlineExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapUnderlineExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapUnderlineExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapUnderlineExtensionApi);
	});

	it('returns underline extension', () => {
		const extensions = extension.getTiptapExtensions();
		expect(extensions).to.be.an('array');
		expect(extensions).to.have.length(1);
		// The extension should be the Underline extension from Tiptap
		expect(extensions[0]).to.exist;
	});
});

describe('UmbTiptapLinkExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapLinkExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapLinkExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapLinkExtensionApi);
	});

	it('returns configured link extension', () => {
		const extensions = extension.getTiptapExtensions();
		expect(extensions).to.be.an('array');
		expect(extensions).to.have.length(1);
		// The extension should be the UmbLink extension from Tiptap configured with openOnClick: false
		expect(extensions[0]).to.exist;
	});
});

describe('UmbTiptapWordCountExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapWordCountExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapWordCountExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapWordCountExtensionApi);
	});

	it('returns word count extension', () => {
		const extensions = extension.getTiptapExtensions();
		expect(extensions).to.be.an('array');
		expect(extensions).to.have.length(1);
		// The extension should be the CharacterCount extension from Tiptap
		expect(extensions[0]).to.exist;
	});

	it('provides word count styles', () => {
		const styles = extension.getStyles();
		expect(styles).to.exist;
		// Should return CSS styles for word count display
	});
});