import { UmbDonutChartElement } from './donut-chart.element.js';
import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { sendKeys } from '@web/test-runner-commands';
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

function detailsText(element: UmbDonutChartElement): string | undefined {
	const box = detailsBox(element);
	return box.classList.contains('show') ? box.textContent!.replace(/\s+/g, ' ').trim() : undefined;
}

function tap(element: UmbDonutChartElement, index: number) {
	const path = listItems(element)[index].querySelector('path')!;
	const bounds = path.getBoundingClientRect();
	const init = {
		bubbles: true,
		composed: true,
		pointerType: 'touch',
		clientX: bounds.left + bounds.width / 2,
		clientY: bounds.top + bounds.height / 2,
	};
	for (const type of ['pointerover', 'pointerdown', 'pointerup', 'pointerout']) {
		path.dispatchEvent(new PointerEvent(type, init));
	}
	svgOf(element).dispatchEvent(new PointerEvent('pointerleave', { ...init, bubbles: false, composed: false }));
}

async function captureWarnings(action: (warnings: Array<string>) => Promise<void>) {
	const warnings: Array<string> = [];
	const originalWarn = console.warn;
	console.warn = (message: string) => warnings.push(message);
	try {
		await action(warnings);
	} finally {
		console.warn = originalWarn;
	}
}

async function tabStopsBetween(first: HTMLElement, last: HTMLElement): Promise<Array<string>> {
	first.focus();
	const stops: Array<string> = [];
	for (let i = 0; i < 10; i++) {
		await sendKeys({ press: 'Tab' });
		const active = document.activeElement;
		if (active === last) return stops;
		const inner = active?.shadowRoot?.activeElement;
		stops.push(inner ? `${inner.tagName.toLowerCase()} ${inner.getAttribute('aria-label')}` : `${active?.tagName}`);
	}
	return stops;
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

		it('announces the description when nothing is drawable', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart description="Messages by level">
					<umb-donut-slice name="A" amount="0"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			const svg = svgOf(element);
			expect(svg.getAttribute('aria-hidden')).to.be.null;
			expect(svg.getAttribute('role')).to.equal('img');
			expect(svg.getAttribute('aria-label')).to.equal('Messages by level');
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

		it('warns and uses the default minimum slice size when min-slice-percent is invalid', async () => {
			await captureWarnings(async (warnings) => {
				const element = await fixture<HTMLElement>(html`
					<div>
						<umb-donut-chart id="default">
							<umb-donut-slice amount="1000"></umb-donut-slice>
							<umb-donut-slice amount="1"></umb-donut-slice>
						</umb-donut-chart>
						<umb-donut-chart id="invalid">
							<umb-donut-slice amount="1000"></umb-donut-slice>
							<umb-donut-slice amount="1"></umb-donut-slice>
						</umb-donut-chart>
					</div>
				`);
				const reference = element.querySelector<UmbDonutChartElement>('#default')!;
				const invalid = element.querySelector<UmbDonutChartElement>('#invalid')!;
				await settle(reference);
				const tinySlice = (chart: UmbDonutChartElement) => listItems(chart)[1].querySelector('path')!.getAttribute('d');

				for (const value of [NaN, -1, Infinity, -Infinity]) {
					invalid.minSlicePercent = 10;
					await settle(invalid);
					warnings.length = 0;

					invalid.minSlicePercent = value;
					await settle(invalid);

					expect(warnings, `${value}`).to.have.lengthOf(1);
					expect(warnings[0], `${value}`).to.contain('min-slice-percent');
					expect(tinySlice(invalid), `${value}`).to.equal(tinySlice(reference));
				}

				warnings.length = 0;
				invalid.minSlicePercent = 0;
				await settle(invalid);
				expect(warnings).to.have.lengthOf(0);
				expect(tinySlice(invalid)).not.to.equal(tinySlice(reference));
			});
		});

		it('picks up slices passed through a nested slot', async () => {
			const host = document.createElement('div');
			host.attachShadow({ mode: 'open' }).innerHTML = '<umb-donut-chart><slot></slot></umb-donut-chart>';
			host.innerHTML =
				'<umb-donut-slice name="A" amount="1"></umb-donut-slice><umb-donut-slice name="B" amount="3"></umb-donut-slice>';
			const container = await fixture<HTMLElement>(html`<div></div>`);
			container.append(host);
			const element = host.shadowRoot!.querySelector<UmbDonutChartElement>('umb-donut-chart')!;
			await settle(element);

			expect(sliceNames(element)).to.deep.equal(['A: 1 (25%)', 'B: 3 (75%)']);
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

		it('formats amounts and shares in the language of the element', async () => {
			const element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart lang="da">
					<umb-donut-slice name="Information" amount="50000"></umb-donut-slice>
					<umb-donut-slice name="Warning" amount="1233"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);

			expect(sliceNames(element)).to.deep.equal(['Information: 50.000 (97,6\u00a0%)', 'Warning: 1.233 (2,4\u00a0%)']);
		});

		it('has one tab stop per link slice and none for the chart itself', async () => {
			const container = await fixture<HTMLElement>(html`
				<div>
					<button id="first">First</button>
					<umb-donut-chart>
						<umb-donut-slice name="A" amount="3" href="#a" label="Open A"></umb-donut-slice>
						<umb-donut-slice name="B" amount="2"></umb-donut-slice>
						<umb-donut-slice name="C" amount="1" href="#c" label="Open C"></umb-donut-slice>
					</umb-donut-chart>
					<button id="last">Last</button>
				</div>
			`);
			await settle(container.querySelector<UmbDonutChartElement>('umb-donut-chart')!);

			const stops = await tabStopsBetween(
				container.querySelector<HTMLElement>('#first')!,
				container.querySelector<HTMLElement>('#last')!,
			);

			expect(stops).to.deep.equal(['a Open A', 'a Open C']);
		});

		it('has no tab stop when no slice is a link', async () => {
			const container = await fixture<HTMLElement>(html`
				<div>
					<button id="first">First</button>
					<umb-donut-chart>
						<umb-donut-slice name="A" amount="3"></umb-donut-slice>
						<umb-donut-slice name="B" amount="2"></umb-donut-slice>
					</umb-donut-chart>
					<button id="last">Last</button>
				</div>
			`);
			await settle(container.querySelector<UmbDonutChartElement>('umb-donut-chart')!);

			const stops = await tabStopsBetween(
				container.querySelector<HTMLElement>('#first')!,
				container.querySelector<HTMLElement>('#last')!,
			);

			expect(stops).to.deep.equal([]);
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

	describe('touch', () => {
		let element: UmbDonutChartElement;

		beforeEach(async () => {
			element = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="Apples" amount="6" kind="items"></umb-donut-slice>
					<umb-donut-slice name="Pears" amount="3" kind="items"></umb-donut-slice>
					<umb-donut-slice name="Plums" amount="1" kind="items" href="#plums"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(element);
		});

		it('toggles the details of a tapped slice that is not a link', async () => {
			tap(element, 1);
			await element.updateComplete;
			expect(detailsText(element)).to.equal('Pears 3 items (30%)');

			tap(element, 1);
			await element.updateComplete;
			expect(detailsText(element)).to.be.undefined;
		});

		it('moves the details to another tapped slice', async () => {
			tap(element, 1);
			await element.updateComplete;
			tap(element, 0);
			await element.updateComplete;

			expect(detailsText(element)).to.equal('Apples 6 items (60%)');
		});

		it('closes the details on a tap outside the chart', async () => {
			tap(element, 0);
			await element.updateComplete;

			document.body.dispatchEvent(
				new PointerEvent('pointerdown', { bubbles: true, composed: true, pointerType: 'touch' }),
			);
			await element.updateComplete;

			expect(detailsText(element)).to.be.undefined;
		});

		it('leaves a tapped link slice to its link', async () => {
			tap(element, 2);
			await element.updateComplete;

			expect(detailsText(element)).to.be.undefined;
		});
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

		it('stays inside the chart when the pointer is near any corner', async () => {
			const bounds = element.getBoundingClientRect();
			const corners = [
				[bounds.left + 2, bounds.top + 2],
				[bounds.right - 2, bounds.top + 2],
				[bounds.left + 2, bounds.bottom - 2],
				[bounds.right - 2, bounds.bottom - 2],
			];

			for (const [x, y] of corners) {
				hover(element, 0, x, y);
				await element.updateComplete;

				const box = detailsBox(element).getBoundingClientRect();
				expect(detailsText(element), `${x},${y}`).to.exist;
				expect(box.left, `${x},${y}`).to.be.at.least(bounds.left - 0.5);
				expect(box.top, `${x},${y}`).to.be.at.least(bounds.top - 0.5);
				expect(box.right, `${x},${y}`).to.be.at.most(bounds.right + 0.5);
				expect(box.bottom, `${x},${y}`).to.be.at.most(bounds.bottom + 0.5);
			}
		});

		it('wraps long names inside the chart', async () => {
			element.querySelector<UmbDonutSliceElement>('umb-donut-slice')!.name = 'A'.repeat(200);
			await settle(element);
			hover(element, 0);
			await element.updateComplete;

			expect(detailsBox(element).getBoundingClientRect().width).to.be.at.most(element.getBoundingClientRect().width);
			expect(detailsBox(element).scrollWidth).to.be.at.most(detailsBox(element).clientWidth);
		});

		it('closes on Escape without moving keyboard focus', async () => {
			const link = svgOf(element).querySelectorAll('a')[0];
			link.focus();
			await element.updateComplete;
			expect(detailsText(element)).to.exist;

			await sendKeys({ press: 'Escape' });
			await element.updateComplete;

			expect(detailsText(element)).to.be.undefined;
			expect(element.shadowRoot!.activeElement).to.equal(link);
		});

		it('closes the details of a hovered slice on Escape', async () => {
			hover(element, 1);
			await element.updateComplete;
			expect(detailsText(element)).to.exist;

			document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
			await element.updateComplete;

			expect(detailsText(element)).to.be.undefined;
		});

		it('draws a two-tone focus ring around a slice with keyboard focus', async () => {
			svgOf(element).querySelectorAll('a')[1].focus();
			await element.updateComplete;

			const ring = [...svgOf(element).querySelectorAll('.focus-ring path')].map((path) => getComputedStyle(path));
			expect(ring).to.have.lengthOf(2);
			expect(ring[0].stroke).not.to.equal(ring[1].stroke);
			expect(parseFloat(ring[0].strokeWidth)).to.be.greaterThan(parseFloat(ring[1].strokeWidth));
		});

		it('keeps keyboard focus and its details on the same slice when an earlier slice is removed', async () => {
			const chart = await fixture<UmbDonutChartElement>(html`
				<umb-donut-chart>
					<umb-donut-slice name="A" amount="3" href="#a" label="Open A"></umb-donut-slice>
					<umb-donut-slice name="B" amount="2" href="#b" label="Open B"></umb-donut-slice>
					<umb-donut-slice name="C" amount="1" href="#c" label="Open C"></umb-donut-slice>
				</umb-donut-chart>
			`);
			await settle(chart);
			svgOf(chart).querySelectorAll('a')[1].focus();
			await chart.updateComplete;

			chart.querySelector('umb-donut-slice')!.remove();
			await settle(chart);

			expect(chart.shadowRoot!.activeElement?.getAttribute('aria-label')).to.equal('Open B');
			expect(detailsText(chart)).to.equal('B 2 (66.7%)');
		});

		it('forgets the hovered slice when it is disconnected', async () => {
			hover(element, 1);
			await element.updateComplete;
			expect(detailsText(element)).to.exist;

			const parent = element.parentElement!;
			element.remove();
			parent.append(element);
			await settle(element);

			expect(detailsText(element)).to.be.undefined;
			expect(svgOf(element).classList.contains('has-active')).to.be.false;
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
