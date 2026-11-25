import { UmbDonutChartElement } from './donut-chart.element.js';
import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

import './donut-slice.element.js';

async function settle(element: UmbDonutChartElement) {
	await aTimeout(0);
	await element.updateComplete;
}

function svgOf(element: UmbDonutChartElement): SVGSVGElement {
	const svg = element.shadowRoot!.querySelector('svg');
	if (!svg) throw new Error('No svg rendered');
	return svg;
}

function paintedBounds(element: UmbDonutChartElement) {
	const boxes = [...svgOf(element).querySelectorAll('path')].map((path) => path.getBBox());
	const left = Math.min(...boxes.map((box) => box.x));
	const top = Math.min(...boxes.map((box) => box.y));
	const right = Math.max(...boxes.map((box) => box.x + box.width));
	const bottom = Math.max(...boxes.map((box) => box.y + box.height));
	return { width: right - left, height: bottom - top };
}

function listItems(element: UmbDonutChartElement) {
	return [...svgOf(element).querySelectorAll('[role="listitem"]')];
}

function sliceNames(element: UmbDonutChartElement) {
	return listItems(element).map(
		(item) => item.querySelector('a, [role="img"]')?.getAttribute('aria-label') ?? item.getAttribute('aria-label'),
	);
}

function hover(element: UmbDonutChartElement, index: number, clientX?: number, clientY?: number) {
	const path = listItems(element)[index].querySelector('path')!;
	const bounds = path.getBoundingClientRect();
	const init = {
		bubbles: true,
		composed: true,
		clientX: clientX ?? bounds.left + bounds.width / 2,
		clientY: clientY ?? bounds.top + bounds.height / 2,
	};
	path.dispatchEvent(new PointerEvent('pointerover', init));
	path.dispatchEvent(new PointerEvent('pointermove', init));
}

function detailsBox(element: UmbDonutChartElement): HTMLElement {
	return element.shadowRoot!.querySelector<HTMLElement>('#details-box')!;
}

describe('UmbDonutChartElement', () => {
	it('is the element defined for its tag', () => {
		expect(customElements.get('umb-donut-chart')).to.equal(UmbDonutChartElement);
	});

	describe('rendering', () => {
		it('draws a single drawable slice as a complete ring that can be hovered and followed', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="Only" amount="7" href="#only"></umb-donut-slice>
					<umb-donut-slice name="Empty" amount="0"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const bounds = paintedBounds(element);
			expect(bounds.width).to.be.closeTo(90, 0.5);
			expect(bounds.height).to.be.closeTo(90, 0.5);
			expect(svgOf(element).querySelector('a')?.getAttribute('href')).to.equal('#only');
		});

		it('shows a neutral track ring without slices when nothing is drawable', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="A" amount="0"></umb-donut-slice>
					<umb-donut-slice name="B" amount="-3"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const svg = svgOf(element);
			expect(svg.querySelectorAll('path.track')).to.have.lengthOf(1);
			expect(svg.querySelectorAll('[role="listitem"], a')).to.have.lengthOf(0);
			expect(svg.getAttribute('aria-hidden')).to.equal('true');
			expect(paintedBounds(element).height).to.be.closeTo(90, 0.5);
		});

		it('shows the track ring when there are no slices', async () => {
			const element = await fixture<UmbDonutChartElement>(html`<umb-donut-chart></umb-donut-chart>`);
			await settle(element);

			expect(svgOf(element).querySelectorAll('path.track')).to.have.lengthOf(1);
		});

		it('sizes the view box from svg-size', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart svg-size="200" radius="90" border-size="30">
					<umb-donut-slice amount="1"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			expect(svgOf(element).getAttribute('viewBox')).to.equal('0 0 200 200');
			expect(paintedBounds(element).width).to.be.closeTo(180, 0.5);
		});

		it('clamps invalid geometry with a warning instead of throwing', async () => {
			const warnings: Array<string> = [];
			const originalWarn = console.warn;
			console.warn = (message: string) => warnings.push(message);
			try {
				const element = await fixture<UmbDonutChartElement>(html`
					<umb-donut-chart radius="45" border-size="60">
						<umb-donut-slice amount="1"></umb-donut-slice>
						<umb-donut-slice amount="2"></umb-donut-slice>
					</umb-donut-chart>
				`);
				await settle(element);
				expect(warnings).to.have.lengthOf(1);

				element.svgSize = -10;
				await element.updateComplete;

				expect(warnings).to.have.lengthOf(3);
				const paths = [...svgOf(element).querySelectorAll('path')].map((path) => path.getAttribute('d'));
				expect(paths).to.have.lengthOf(2);
				paths.forEach((d) => expect(d).not.to.contain('NaN'));
				expect(svgOf(element).getAttribute('viewBox')).to.equal('0 0 100 100');
			} finally {
				console.warn = originalWarn;
			}
		});

		it('resolves CSS custom properties used as slice colours', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart style="--test-slice-color: rgb(1, 2, 3)">
					<umb-donut-slice amount="1" color="var(--test-slice-color)"></umb-donut-slice>
					<umb-donut-slice amount="1" color="rgb(4, 5, 6)"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const fills = [...svgOf(element).querySelectorAll('path')].map((path) => getComputedStyle(path).fill);
			expect(fills).to.deep.equal(['rgb(1, 2, 3)', 'rgb(4, 5, 6)']);
		});

		it('gives each chart its own minimum slice size', async () => {
			const element = await fixture<HTMLElement>(html`
				<div>
					<umb-donut-chart id="small">
						<umb-donut-slice amount="1000"></umb-donut-slice>
						<umb-donut-slice amount="1"></umb-donut-slice>
					</umb-donut-chart>
					<umb-donut-chart id="large" min-slice-percent="10">
						<umb-donut-slice amount="1000"></umb-donut-slice>
						<umb-donut-slice amount="1"></umb-donut-slice>
					</umb-donut-chart>
				</div>
			`);
			const small = element.querySelector<UmbDonutChartElement>('#small')!;
			const large = element.querySelector<UmbDonutChartElement>('#large')!;
			await settle(small);
			await settle(large);

			const tinySlice = (chart: UmbDonutChartElement) => listItems(chart)[1].querySelector('path')!.getBBox();
			expect(tinySlice(large).width * tinySlice(large).height).to.be.greaterThan(
				tinySlice(small).width * tinySlice(small).height * 4,
			);
		});

		it('follows slices that are added, changed and removed', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="A" amount="1"></umb-donut-slice>
					<umb-donut-slice name="B" amount="1"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const [first, second] = element.querySelectorAll<UmbDonutSliceElement>('umb-donut-slice');
			first.amount = 3;
			second.name = 'Bee';
			const added = document.createElement('umb-donut-slice');
			added.name = 'C';
			added.amount = 4;
			element.append(added);
			await settle(element);
			expect(sliceNames(element)).to.deep.equal(['A: 3 (37.5%)', 'Bee: 1 (12.5%)', 'C: 4 (50%)']);

			first.remove();
			await settle(element);
			expect(sliceNames(element)).to.deep.equal(['Bee: 1 (20%)', 'C: 4 (80%)']);
		});

		it('applies changes to many slices in a single update', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					${Array.from({ length: 10 }, () => html`<umb-donut-slice amount="1"></umb-donut-slice>`)}
				</umb-donut-chart>
			`);
			await settle(element);
			const render = element.render.bind(element);
			let renders = 0;
			element.render = () => {
				renders++;
				return render();
			};

			element.querySelectorAll<UmbDonutSliceElement>('umb-donut-slice').forEach((slice, i) => (slice.amount = i + 1));
			await settle(element);

			expect(renders).to.equal(1);
			expect(sliceNames(element).at(-1)).to.equal('10 (18.2%)');
		});
	});

	describe('accessibility', () => {
		it('names each drawable slice once with its name, amount, kind and true share', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart description="Messages by level">
					<umb-donut-slice name="Information" amount="50000" kind="messages"></umb-donut-slice>
					<umb-donut-slice name="Warning" amount="1200" kind="messages"></umb-donut-slice>
					<umb-donut-slice name="Empty" amount="0" kind="messages"></umb-donut-slice>
					<umb-donut-slice name="Error" amount="30" kind="messages"></umb-donut-slice>
					<umb-donut-slice name="Fatal" amount="1" kind="messages"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			expect(svgOf(element).getAttribute('role')).to.equal('list');
			expect(svgOf(element).getAttribute('aria-label')).to.equal('Messages by level');
			expect(sliceNames(element)).to.deep.equal([
				'Information: 50,000 messages (97.6%)',
				'Warning: 1,200 messages (2.3%)',
				'Error: 30 messages (0.1%)',
				'Fatal: 1 messages (<0.1%)',
			]);
		});

		it('names a link slice by its label and keeps the slice details as its description', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="A" amount="1" kind="items" href="#a" label="Open A"></umb-donut-slice>
					<umb-donut-slice name="B" amount="3" kind="items" href="#b"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const [linkA, linkB] = [...svgOf(element).querySelectorAll('a')];
			expect(linkA.getAttribute('aria-label')).to.equal('Open A');
			expect(linkA.querySelector('desc')?.textContent).to.equal('A: 1 items (25%)');
			expect(linkB.getAttribute('aria-label')).to.equal('B: 3 items (75%)');
		});

		if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
			it('passes the a11y audit', async () => {
				const element = await fixture<UmbDonutChartElement>(html`
					<umb-donut-chart description="Fruit">
						<umb-donut-slice name="Apples" amount="3" href="#apples"></umb-donut-slice>
						<umb-donut-slice name="Pears" amount="1"></umb-donut-slice>
					</umb-donut-chart>
				`);
				await settle(element);

				await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
			});
		}
	});

	describe('details box', () => {
		let element: UmbDonutChartElement;

		beforeEach(async () => {
			element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="Information" amount="50000" kind="messages" href="#info"></umb-donut-slice>
					<umb-donut-slice name="Fatal" amount="1" kind="messages" href="#fatal"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);
		});

		it('shows the name, amount, kind and true share of a hovered slice', async () => {
			hover(element, 1);
			await element.updateComplete;

			const box = detailsBox(element);
			expect(box.classList.contains('show')).to.be.true;
			expect(box.textContent!.replace(/\s+/g, ' ').trim()).to.equal('Fatal 1 messages (<0.1%)');
		});

		it('shows the details of a link slice that receives keyboard focus', async () => {
			svgOf(element).querySelectorAll('a')[0].focus();
			await element.updateComplete;

			const box = detailsBox(element);
			expect(box.classList.contains('show')).to.be.true;
			expect(box.textContent!.replace(/\s+/g, ' ').trim()).to.equal('Information 50,000 messages (>99.9%)');
		});

		it('stays inside the chart when the pointer is near an edge', async () => {
			const bounds = element.getBoundingClientRect();
			hover(element, 0, bounds.right - 2, bounds.bottom - 2);
			await element.updateComplete;

			const box = detailsBox(element).getBoundingClientRect();
			expect(box.left).to.be.at.least(bounds.left - 0.5);
			expect(box.top).to.be.at.least(bounds.top - 0.5);
			expect(box.right).to.be.at.most(bounds.right + 0.5);
			expect(box.bottom).to.be.at.most(bounds.bottom + 0.5);
		});

		it('hides when the pointer leaves the chart', async () => {
			hover(element, 0);
			await element.updateComplete;
			svgOf(element).dispatchEvent(new PointerEvent('pointerleave'));
			await element.updateComplete;

			expect(detailsBox(element).classList.contains('show')).to.be.false;
		});
	});
});
