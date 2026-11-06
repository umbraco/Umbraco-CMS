import { UmbTiptapStatusbarElementPathElement } from './element-path.tiptap-statusbar-element.js';
import { expect, fixture, html } from '@open-wc/testing';
import type { Editor } from '../../externals.js';

describe('UmbTiptapStatusbarElementPathElement', () => {
	for (const dir of ['ltr', 'rtl'] as const) {
		describe(`in a ${dir} layout`, () => {
			let element: UmbTiptapStatusbarElementPathElement;

			beforeEach(async () => {
				const editorDom = await fixture<HTMLDivElement>(
					html`<div>
						<p class="lead"><strong>Text</strong></p>
					</div>`,
				);
				const editor = {
					on: () => {},
					off: () => {},
					state: { selection: { from: 0 } },
					view: { dom: editorDom, domAtPos: () => ({ node: editorDom.querySelector('strong')!.firstChild }) },
				} as unknown as Editor;

				const container = await fixture<HTMLDivElement>(html`<div dir=${dir}></div>`);
				element = document.createElement('umb-tiptap-statusbar-element-path');
				element.editor = editor;
				container.appendChild(element);
				await element.updateComplete;
			});

			it('lays out each path item left to right', () => {
				for (const code of element.shadowRoot!.querySelectorAll('code')) {
					expect(getComputedStyle(code).direction).to.equal('ltr');
				}
			});

			it('places a separator between consecutive path items', () => {
				const [parent, child] = [...element.shadowRoot!.querySelectorAll('code')].map((code) =>
					code.getBoundingClientRect(),
				);
				const separator = element.shadowRoot!.querySelector('code + :not(code)')?.getBoundingClientRect();

				expect(separator).to.exist;
				const [first, second] = dir === 'rtl' ? [child, parent] : [parent, child];
				expect(separator!.left).to.be.at.least(first.right);
				expect(separator!.right).to.be.at.most(second.left);
			});
		});
	}
});
