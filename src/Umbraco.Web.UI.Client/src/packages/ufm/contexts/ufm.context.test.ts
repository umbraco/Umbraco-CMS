import { expect } from '@open-wc/testing';
import { UmbMarked } from './ufm.context.js';

describe('UmbMarked sanitization', () => {
	describe('XSS prevention on custom elements', () => {
		it('should strip onclick from custom elements', async () => {
			const markup = await UmbMarked.parseInline('<uui-button onclick="alert(1)">Click</uui-button>');
			expect(markup).to.not.include('onclick');
			expect(markup).to.include('<uui-button>');
		});

		it('should strip onload from custom elements', async () => {
			const markup = await UmbMarked.parseInline('<umb-test onload="alert(1)">Test</umb-test>');
			expect(markup).to.not.include('onload');
			expect(markup).to.include('<umb-test>');
		});

		it('should strip onmouseover from custom elements', async () => {
			const markup = await UmbMarked.parseInline('<uui-box onmouseover="alert(1)">Hover</uui-box>');
			expect(markup).to.not.include('onmouseover');
			expect(markup).to.include('<uui-box>');
		});

		it('should strip onfocus from custom elements', async () => {
			const markup = await UmbMarked.parseInline('<ufm-label-value onfocus="alert(1)" alias="test"></ufm-label-value>');
			expect(markup).to.not.include('onfocus');
			expect(markup).to.include('alias="test"');
		});

		it('should strip onerror from custom elements', async () => {
			const markup = await UmbMarked.parseInline('<umb-test onerror="alert(1)">Test</umb-test>');
			expect(markup).to.not.include('onerror');
		});
	});

	describe('safe attributes on custom elements', () => {
		it('should preserve class attribute', async () => {
			const markup = await UmbMarked.parseInline('<uui-button class="primary">Click</uui-button>');
			expect(markup).to.include('class="primary"');
		});

		it('should preserve alias attribute', async () => {
			const markup = await UmbMarked.parseInline('<ufm-label-value alias="prop1"></ufm-label-value>');
			expect(markup).to.include('alias="prop1"');
		});

		it('should preserve look attribute', async () => {
			const markup = await UmbMarked.parseInline('<uui-button look="primary">Click</uui-button>');
			expect(markup).to.include('look="primary"');
		});

		it('should preserve slot attribute', async () => {
			const markup = await UmbMarked.parseInline('<umb-test slot="header">Test</umb-test>');
			expect(markup).to.include('slot="header"');
		});

		it('should preserve label attribute', async () => {
			const markup = await UmbMarked.parseInline('<uui-button label="Submit">Click</uui-button>');
			expect(markup).to.include('label="Submit"');
		});
	});

	describe('standard HTML sanitization', () => {
		it('should strip script tags', async () => {
			const markup = await UmbMarked.parseInline('<script>alert(1)</script>');
			expect(markup).to.not.include('<script');
		});

		it('should allow standard markdown bold', async () => {
			const markup = await UmbMarked.parseInline('**bold text**');
			expect(markup).to.include('<strong>bold text</strong>');
		});

		it('should allow standard markdown links', async () => {
			const markup = await UmbMarked.parseInline('[link](https://example.com)');
			expect(markup).to.include('<a');
			expect(markup).to.include('href="https://example.com"');
		});

		it('should strip non-allowed custom elements', async () => {
			const markup = await UmbMarked.parseInline('<custom-evil onclick="alert(1)">Test</custom-evil>');
			expect(markup).to.not.include('onclick');
			expect(markup).to.not.include('<custom-evil');
		});
	});
});
