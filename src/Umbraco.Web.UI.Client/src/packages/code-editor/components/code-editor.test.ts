import { UmbCodeEditorElement } from './code-editor.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbCodeEditorElement', () => {
	it('lays out the editor left to right in a right-to-left document', async () => {
		const container = await fixture<HTMLDivElement>(html`<div dir="rtl"><umb-code-editor></umb-code-editor></div>`);
		const element = container.querySelector<UmbCodeEditorElement>('umb-code-editor')!;
		await element.updateComplete;

		expect(getComputedStyle(element.shadowRoot!.querySelector('#editor-container')!).direction).to.equal('ltr');
	});
});
