import { UmbFigureCardElement } from './figure-card.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbFigureCardElement', () => {
	let element: UmbFigureCardElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-figure-card name="Card Name"></umb-figure-card>`);
	});

	function childrenIndicator(): Element | null {
		return element.shadowRoot!.querySelector('#has-children');
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbFigureCardElement);
	});

	it('renders no children indicator by default', () => {
		expect(childrenIndicator()).to.be.null;
	});

	it('renders a children indicator when it has children', async () => {
		element.hasChildren = true;
		await element.updateComplete;

		expect(childrenIndicator()).to.exist;
	});

	it('renders a children indicator when the has-children attribute is set', async () => {
		element.setAttribute('has-children', '');
		await element.updateComplete;

		expect(childrenIndicator()).to.exist;
	});

	it('hides the children indicator from assistive technology', async () => {
		element.hasChildren = true;
		await element.updateComplete;

		expect(childrenIndicator()!.getAttribute('aria-hidden')).to.equal('true');
	});
});

describe('UmbFigureCardElement click targets', () => {
	const EDGE_OFFSET = 4;

	type ClickPoint = 'name' | 'portrait centre' | 'top edge' | 'bottom edge' | 'side edge';

	async function setup(attributes: { selectable?: boolean; selectOnly?: boolean; hasChildren?: boolean }) {
		const wrapper = await fixture<HTMLDivElement>(html`
			<div style="width: 240px; height: 320px;">
				<umb-figure-card
					name="Card Name"
					description="Card description"
					?selectable=${attributes.selectable}
					?select-only=${attributes.selectOnly}
					?has-children=${attributes.hasChildren}>
					<div style="width: 80px; height: 80px;"></div>
				</umb-figure-card>
			</div>
		`);
		const card = wrapper.querySelector<UmbFigureCardElement>('umb-figure-card')!;
		await card.updateComplete;
		return card;
	}

	function pointFor(card: UmbFigureCardElement, point: ClickPoint): { x: number; y: number } {
		const cardRect = card.getBoundingClientRect();
		const nameRect = card.shadowRoot!.querySelector('#name')!.getBoundingClientRect();
		const centreX = cardRect.left + cardRect.width / 2;
		switch (point) {
			case 'name':
				return { x: nameRect.left + 4, y: nameRect.top + nameRect.height / 2 };
			case 'portrait centre':
				return { x: centreX, y: cardRect.top + 60 };
			case 'top edge':
				return { x: centreX, y: cardRect.top + EDGE_OFFSET };
			case 'bottom edge':
				return { x: centreX, y: cardRect.bottom - EDGE_OFFSET };
			case 'side edge':
				return { x: cardRect.left + EDGE_OFFSET, y: nameRect.top + nameRect.height / 2 };
		}
	}

	/** Clicks whatever the browser hit-tests at the given point, the same element a pointer click would reach. */
	function clickAt(card: UmbFigureCardElement, point: ClickPoint): Array<string> {
		const { x, y } = pointFor(card, point);
		const target = card.shadowRoot!.elementFromPoint(x, y) as HTMLElement | null;
		expect(target, `nothing is hit at the ${point}`).to.exist;

		const events: Array<string> = [];
		const record = (event: Event) => events.push(event.type);
		card.addEventListener('open', record);
		card.addEventListener('selected', record);
		card.addEventListener('deselected', record);
		target!.click();
		card.removeEventListener('open', record);
		card.removeEventListener('selected', record);
		card.removeEventListener('deselected', record);
		return events;
	}

	describe('when not selectable', () => {
		const points: Array<ClickPoint> = ['name', 'portrait centre', 'top edge', 'bottom edge', 'side edge'];
		for (const point of points) {
			it(`opens on a click at the ${point}`, async () => {
				const card = await setup({});
				expect(clickAt(card, point)).to.deep.equal(['open']);
			});
		}
	});

	describe('when selectable', () => {
		for (const point of ['name', 'portrait centre'] as Array<ClickPoint>) {
			it(`opens on a click at the ${point}`, async () => {
				const card = await setup({ selectable: true });
				expect(clickAt(card, point)).to.deep.equal(['open']);
			});
		}

		for (const point of ['top edge', 'bottom edge', 'side edge'] as Array<ClickPoint>) {
			it(`selects on a click at the ${point}`, async () => {
				const card = await setup({ selectable: true });
				expect(clickAt(card, point)).to.deep.equal(['selected']);
			});
		}

		it('selects on a click at the bottom edge when it has children', async () => {
			const card = await setup({ selectable: true, hasChildren: true });
			expect(clickAt(card, 'bottom edge')).to.deep.equal(['selected']);
		});
	});

	describe('when select-only', () => {
		const points: Array<ClickPoint> = ['name', 'portrait centre', 'top edge', 'bottom edge', 'side edge'];
		for (const point of points) {
			it(`selects on a click at the ${point}`, async () => {
				const card = await setup({ selectable: true, selectOnly: true });
				expect(clickAt(card, point)).to.deep.equal(['selected']);
			});
		}
	});
});

describe('UmbFigureCardElement stacking', () => {
	it('keeps its layers below a z-index: 1 overlay that follows it', async () => {
		const wrapper = await fixture<HTMLDivElement>(html`
			<div style="position: relative; width: 240px; height: 320px;">
				<umb-figure-card name="Card Name" description="Card description" selectable>
					<div style="width: 80px; height: 80px;"></div>
				</umb-figure-card>
				<div id="overlay" style="position: absolute; inset: 0; z-index: 1;"></div>
			</div>
		`);
		const card = wrapper.querySelector<UmbFigureCardElement>('umb-figure-card')!;
		await card.updateComplete;
		const overlay = wrapper.querySelector('#overlay');

		const cardRect = card.getBoundingClientRect();
		const nameRect = card.shadowRoot!.querySelector('#name')!.getBoundingClientRect();
		const points = [
			{ label: 'name', x: nameRect.left + 4, y: nameRect.top + nameRect.height / 2 },
			{ label: 'portrait centre', x: cardRect.left + cardRect.width / 2, y: cardRect.top + 60 },
			{ label: 'bottom edge', x: cardRect.left + cardRect.width / 2, y: cardRect.bottom - 4 },
		];
		for (const { label, x, y } of points) {
			const hit = document.elementFromPoint(x, y);
			expect(hit === overlay, `${hit?.tagName.toLowerCase()} is painted above the overlay at the ${label}`).to.be.true;
		}
	});
});
