import { UmbFigureCardElement } from './figure-card.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { ifDefined } from '@umbraco-cms/backoffice/external/lit';

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


const EDGE_OFFSET = 4;

type ClickPoint = 'name' | 'portrait centre' | 'top edge' | 'bottom edge' | 'side edge';

interface CardSetup {
	selectable?: boolean;
	selectOnly?: boolean;
	hasChildren?: boolean;
	disabled?: boolean;
	href?: string;
	withAction?: boolean;
}

async function setupCard(attributes: CardSetup) {
	const wrapper = await fixture<HTMLDivElement>(html`
		<div style="width: 240px; height: 320px;">
			<umb-figure-card
				name="Card Name"
				description="Card description"
				href=${ifDefined(attributes.href)}
				?selectable=${attributes.selectable}
				?select-only=${attributes.selectOnly}
				?has-children=${attributes.hasChildren}
				?disabled=${attributes.disabled}>
				<div style="width: 80px; height: 80px;"></div>
				${attributes.withAction ? html`<button slot="actions" style="width: 32px; height: 32px;">Action</button>` : ''}
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

/**
 * Runs an interaction and reports what the card did: `open`, `selected` and `deselected` events, plus `navigate`
 * when the click would follow a link. Navigation itself is prevented so the test page stays put.
 */
function recordCard(card: UmbFigureCardElement, interact: () => void): Array<string> {
	const events: Array<string> = [];
	const record = (event: Event) => events.push(event.type);
	const recordNavigation = (event: Event) => {
		const link = event.composedPath().find((el) => el instanceof HTMLAnchorElement && el.hasAttribute('href'));
		if (!link) return;
		event.preventDefault();
		events.push('navigate');
	};
	const types = ['open', 'selected', 'deselected'];
	types.forEach((type) => card.addEventListener(type, record));
	document.addEventListener('click', recordNavigation);
	try {
		interact();
	} finally {
		types.forEach((type) => card.removeEventListener(type, record));
		document.removeEventListener('click', recordNavigation);
	}
	return events;
}

/** Clicks whatever the browser hit-tests at the given point, the same element a pointer click would reach. */
function clickAt(card: UmbFigureCardElement, point: ClickPoint): Array<string> {
	const { x, y } = pointFor(card, point);
	const target = card.shadowRoot!.elementFromPoint(x, y) as HTMLElement | null;
	expect(target, `nothing is hit at the ${point}`).to.exist;
	return recordCard(card, () => target!.click());
}

function pressKey(card: UmbFigureCardElement, target: EventTarget, key: 'Enter' | ' '): Array<string> {
	const code = key === 'Enter' ? 'Enter' : 'Space';
	return recordCard(card, () =>
		target.dispatchEvent(new KeyboardEvent('keydown', { key, code, bubbles: true, composed: true, cancelable: true })),
	);
}

function openPart(card: UmbFigureCardElement): HTMLElement {
	return card.shadowRoot!.querySelector<HTMLElement>('#open-part')!;
}

describe('UmbFigureCardElement click targets', () => {
	describe('when not selectable', () => {
		const points: Array<ClickPoint> = ['name', 'portrait centre', 'top edge', 'bottom edge', 'side edge'];
		for (const point of points) {
			it(`opens on a click at the ${point}`, async () => {
				const card = await setupCard({});
				expect(clickAt(card, point)).to.deep.equal(['open']);
			});
		}
	});

	describe('when selectable', () => {
		for (const point of ['name', 'portrait centre'] as Array<ClickPoint>) {
			it(`opens on a click at the ${point}`, async () => {
				const card = await setupCard({ selectable: true });
				expect(clickAt(card, point)).to.deep.equal(['open']);
			});
		}

		for (const point of ['top edge', 'bottom edge', 'side edge'] as Array<ClickPoint>) {
			it(`selects on a click at the ${point}`, async () => {
				const card = await setupCard({ selectable: true });
				expect(clickAt(card, point)).to.deep.equal(['selected']);
			});
		}

		it('selects on a click at the bottom edge when it has children', async () => {
			const card = await setupCard({ selectable: true, hasChildren: true });
			expect(clickAt(card, 'bottom edge')).to.deep.equal(['selected']);
		});
	});

	describe('when select-only', () => {
		const points: Array<ClickPoint> = ['name', 'portrait centre', 'top edge', 'bottom edge', 'side edge'];
		for (const point of points) {
			it(`selects on a click at the ${point}`, async () => {
				const card = await setupCard({ selectable: true, selectOnly: true });
				expect(clickAt(card, point)).to.deep.equal(['selected']);
			});
		}
	});
});

describe('UmbFigureCardElement slotted actions', () => {
	for (const mode of [{}, { selectable: true }] as Array<CardSetup>) {
		const label = mode.selectable ? 'a selectable card' : 'a card';
		it(`runs the action without opening or selecting ${label}`, async () => {
			const card = await setupCard({ ...mode, withAction: true });
			const action = card.querySelector<HTMLButtonElement>('[slot="actions"]')!;
			const rect = action.getBoundingClientRect();
			const hit = document.elementFromPoint(rect.left + rect.width / 2, rect.top + rect.height / 2);
			expect(hit === action, `${hit?.tagName.toLowerCase()} is hit instead of the action`).to.be.true;

			let actionClicks = 0;
			action.addEventListener('click', () => actionClicks++);
			expect(recordCard(card, () => action.click())).to.deep.equal([]);
			expect(actionClicks).to.equal(1);
		});
	}
});

describe('UmbFigureCardElement when disabled', () => {
	for (const point of ['name', 'portrait centre', 'bottom edge'] as Array<ClickPoint>) {
		it(`does not open on a click at the ${point}`, async () => {
			const card = await setupCard({ disabled: true });
			expect(clickAt(card, point)).to.not.include('open');
		});
	}

	it('does not open on Enter', async () => {
		const card = await setupCard({ disabled: true });
		expect(pressKey(card, openPart(card), 'Enter')).to.not.include('open');
	});

	it('does not navigate when it has a link', async () => {
		const card = await setupCard({ disabled: true, href: '#figure-card-link' });
		expect(openPart(card).hasAttribute('href')).to.be.false;
		expect(clickAt(card, 'name')).to.not.include('navigate');
	});
});

describe('UmbFigureCardElement keyboard', () => {
	it('opens on Enter on the open part', async () => {
		const card = await setupCard({});
		expect(pressKey(card, openPart(card), 'Enter')).to.deep.equal(['open']);
	});

	it('opens, and does not select, on Enter on the open part of a selectable card', async () => {
		const card = await setupCard({ selectable: true });
		expect(pressKey(card, openPart(card), 'Enter')).to.deep.equal(['open']);
	});

	for (const key of ['Enter', ' '] as const) {
		it(`selects on ${key === ' ' ? 'Space' : key} on a selectable card`, async () => {
			const card = await setupCard({ selectable: true });
			expect(pressKey(card, card, key)).to.deep.equal(['selected']);
		});
	}

	it('makes a selectable card focusable', async () => {
		const card = await setupCard({ selectable: true });
		expect(card.getAttribute('tabindex')).to.equal('0');
	});

	it('keeps the open part in the tab order', async () => {
		const card = await setupCard({});
		expect(openPart(card).getAttribute('tabindex')).to.equal('0');
	});

	it('takes the open part out of the tab order when select-only', async () => {
		const card = await setupCard({ selectable: true, selectOnly: true });
		expect(openPart(card).getAttribute('tabindex')).to.equal('-1');
	});
});

describe('UmbFigureCardElement with a link', () => {
	const HREF = '#figure-card-link';

	it('renders the open part as a link', async () => {
		const card = await setupCard({ href: HREF });
		expect(openPart(card).tagName).to.equal('A');
		expect(openPart(card).getAttribute('href')).to.equal(HREF);
	});

	for (const point of ['name', 'portrait centre', 'bottom edge'] as Array<ClickPoint>) {
		it(`navigates on a click at the ${point}`, async () => {
			const card = await setupCard({ href: HREF });
			expect(clickAt(card, point)).to.deep.equal(['navigate']);
		});
	}

	it('navigates on a click at the name of a selectable card', async () => {
		const card = await setupCard({ href: HREF, selectable: true });
		expect(clickAt(card, 'name')).to.deep.equal(['navigate']);
	});

	for (const point of ['top edge', 'bottom edge', 'side edge'] as Array<ClickPoint>) {
		it(`selects, and does not navigate, on a click at the ${point} of a selectable card`, async () => {
			const card = await setupCard({ href: HREF, selectable: true });
			expect(clickAt(card, point)).to.deep.equal(['selected']);
		});
	}

	it('selects, and does not navigate, on a click at the name when select-only', async () => {
		const card = await setupCard({ href: HREF, selectable: true, selectOnly: true });
		expect(clickAt(card, 'name')).to.deep.equal(['selected']);
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
