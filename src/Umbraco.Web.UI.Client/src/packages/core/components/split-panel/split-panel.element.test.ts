import { UmbSplitPanelElement } from './split-panel.element.js';
import { expect, fixture, html } from '@open-wc/testing';

const nextFrame = () => new Promise((resolve) => requestAnimationFrame(resolve));

describe('UmbSplitPanelElement', () => {
	for (const dir of ['ltr', 'rtl'] as const) {
		describe(`in a ${dir} layout`, () => {
			let element: UmbSplitPanelElement;
			let container: HTMLDivElement;

			const startPanelWidth = () => container.querySelector('[slot="start"]')!.getBoundingClientRect().width;
			const inlineStartEdge = () => {
				const { left, right } = container.getBoundingClientRect();
				return dir === 'rtl' ? right : left;
			};
			const towardsInlineEnd = dir === 'rtl' ? -1 : 1;

			beforeEach(async () => {
				container = await fixture<HTMLDivElement>(
					html`<div dir=${dir} style="width: 1000px; height: 200px;">
						<umb-split-panel lock="start" style="--umb-split-panel-initial-position: 300px;">
							<div slot="start"></div>
							<div slot="end"></div>
						</umb-split-panel>
					</div>`,
				);
				element = container.querySelector('umb-split-panel')!;
				await nextFrame();
				await nextFrame();
				await element.updateComplete;
			});

			it('sizes the start panel from the initial position', () => {
				expect(element.position).to.equal('30%');
				expect(startPanelWidth()).to.be.closeTo(300, 1);
			});

			it('sizes the start panel from the pointer distance to the inline start while dragging', async () => {
				const touchArea = element.shadowRoot!.querySelector('#divider-touch-area')!;

				touchArea.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true, cancelable: true }));
				document.dispatchEvent(
					new PointerEvent('pointermove', { clientX: inlineStartEdge() + 400 * towardsInlineEnd }),
				);
				document.dispatchEvent(new PointerEvent('pointerup'));
				await element.updateComplete;

				expect(element.position).to.equal('40%');
				expect(startPanelWidth()).to.be.closeTo(400, 1);
			});

			it('moves the divider in the direction of the arrow key', async () => {
				const divider = element.shadowRoot!.querySelector('#divider')!;
				const before = divider.getBoundingClientRect().left;

				element
					.shadowRoot!.querySelector('#divider-touch-area')!
					.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowLeft', bubbles: true }));
				await element.updateComplete;

				expect(divider.getBoundingClientRect().left).to.be.closeTo(before - 10, 1);
			});

			it('extends the divider touch area into the end panel', () => {
				const touchArea = element.shadowRoot!.querySelector('#divider-touch-area')!.getBoundingClientRect();
				const endPanel = container.querySelector('[slot="end"]')!.getBoundingClientRect();
				const overlap = Math.min(touchArea.right, endPanel.right) - Math.max(touchArea.left, endPanel.left);

				expect(overlap).to.be.closeTo(touchArea.width - 1, 1);
			});
		});
	}
});
