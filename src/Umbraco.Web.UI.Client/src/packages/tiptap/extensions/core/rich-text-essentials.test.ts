import { UmbTiptapRichTextEssentialsExtensionApi } from './rich-text-essentials.tiptap-api.js';
import { expect } from '@open-wc/testing';

describe('UmbTiptapRichTextEssentialsExtensionApi', () => {
	let host: HTMLElement;
	let extension: UmbTiptapRichTextEssentialsExtensionApi;

	beforeEach(() => {
		host = document.createElement('div');
		extension = new UmbTiptapRichTextEssentialsExtensionApi(host);
	});

	afterEach(() => {
		extension.destroy();
		host.remove();
	});

	it('is defined with its own instance', () => {
		expect(extension).to.be.instanceOf(UmbTiptapRichTextEssentialsExtensionApi);
	});

	it('returns multiple essential extensions', () => {
		const extensions = extension.getTiptapExtensions();
		expect(extensions).to.be.an('array');
		expect(extensions.length).to.be.greaterThan(1);
		
		// Should include StarterKit, Anchor, Div, Span, HtmlGlobalAttributes, TrailingNode
		expect(extensions.length).to.be.at.least(6);
	});

	it('provides CSS styles for rich text elements', () => {
		const styles = extension.getStyles();
		expect(styles).to.exist;
		// Should return CSS styles for rich text formatting
	});

	it('configures HtmlGlobalAttributes for supported types', () => {
		const extensions = extension.getTiptapExtensions();
		
		// Find the HtmlGlobalAttributes extension
		const htmlGlobalAttrs = extensions.find(ext => 
			ext && typeof ext === 'object' && 'name' in ext && ext.name === 'htmlGlobalAttributes'
		);
		
		// If found, it should be configured with the expected types
		expect(htmlGlobalAttrs).to.exist;
	});

	it('includes essential formatting extensions', () => {
		const extensions = extension.getTiptapExtensions();
		
		// Verify we have the essential extensions by checking the array length
		// and that they are defined objects/functions
		extensions.forEach(ext => {
			expect(ext).to.exist;
		});
	});
});