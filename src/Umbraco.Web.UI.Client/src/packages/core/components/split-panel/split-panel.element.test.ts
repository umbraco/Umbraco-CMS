import { UmbSplitPanelElement } from './split-panel.element.js';
import { expect, fixture, html, nextFrame, oneEvent } from '@open-wc/testing';

describe('UmbSplitPanelElement', () => {
	for (const lock of ['start', 'end'] as const) {
		describe(`with the ${lock} panel locked`, () => {
			let element: UmbSplitPanelElement;
			let container: HTMLDivElement;

			beforeEach(async () => {
				container = await fixture(html`
					<div style="width: 800px; height: 200px">
						<umb-split-panel lock=${lock} position="25%">
							<div slot="start">Start</div>
							<div slot="end">End</div>
						</umb-split-panel>
					</div>
				`);
				element = container.querySelector('umb-split-panel')!;
				await nextFrame();
				await element.updateComplete;
			});

			function expectColumns(start: number, end: number) {
				expect(getComputedStyle(element.mainElement).display).to.equal('grid');
				expect(getComputedStyle(element.mainElement).gridTemplateColumns).to.equal(`${start}px 0px ${end}px`);
				const columns = element.mainElement.style.gridTemplateColumns.replace(/\s+/g, ' ').trim();
				expect(columns).to.equal(
					`minmax(var(--umb-split-panel-start-min-width), ${lock === 'start' ? `${start}px` : '1fr'}) 0px minmax(var(--umb-split-panel-end-min-width), ${lock === 'end' ? `${end}px` : '1fr'})`,
				);
			}

			it('initializes pixel-locked grid columns from a percentage position', () => {
				expect(element).to.be.instanceOf(UmbSplitPanelElement);
				expect(element.position).to.equal('25%');
				expectColumns(200, 600);
			});

			it('reinitializes grid columns after a slot change without changing position', async () => {
				const slot = element.shadowRoot!.querySelector<HTMLSlotElement>('slot[name="end"]')!;
				const changed = oneEvent(slot, 'slotchange');
				container.style.width = '1000px';
				element
					.querySelector('[slot="end"]')!
					.replaceWith(Object.assign(document.createElement('div'), { slot: 'end' }));
				await changed;

				expect(element.position).to.equal('25%');
				expectColumns(250, 750);
				await nextFrame();
				await element.updateComplete;
				expect(element.position).to.equal('25%');
				expectColumns(250, 750);
			});

			it('reinitializes grid columns when the element reconnects without changing position', async () => {
				element.remove();
				container.style.width = '1000px';
				container.append(element);
				await nextFrame();
				await element.updateComplete;

				expect(element.position).to.equal('25%');
				expectColumns(250, 750);
			});

			it('reconnects both panels without changing position', async () => {
				const slot = element.shadowRoot!.querySelector<HTMLSlotElement>('slot[name="end"]')!;
				const removed = oneEvent(slot, 'slotchange');
				const endPanel = element.querySelector('[slot="end"]')!;
				endPanel.remove();
				await removed;
				await element.updateComplete;
				expect(getComputedStyle(element.mainElement).display).to.equal('flex');

				container.style.width = '1000px';
				const added = oneEvent(slot, 'slotchange');
				element.append(endPanel);
				await added;

				expect(element.position).to.equal('25%');
				expectColumns(250, 750);
				await nextFrame();
				await element.updateComplete;
				expect(element.position).to.equal('25%');
				expectColumns(250, 750);
			});

			for (const initialPosition of [undefined, '30%']) {
				it(`initializes locked columns using the ${initialPosition ? 'overridden' : 'documented default'} CSS variable`, async () => {
					const defaultContainer = await fixture<HTMLDivElement>(html`
						<div style="width: 800px; height: 200px">
							<umb-split-panel
								lock=${lock}
								style=${initialPosition ? `--umb-split-panel-initial-position: ${initialPosition}` : ''}>
								<div slot="start">Start</div>
								<div slot="end">End</div>
							</umb-split-panel>
						</div>
					`);
					element = defaultContainer.querySelector('umb-split-panel')!;
					await nextFrame();
					await element.updateComplete;

					expect(element.position).to.equal(initialPosition ?? '50%');
					expectColumns(initialPosition ? 240 : 400, initialPosition ? 560 : 400);
				});
			}
		});
	}
});
