import type { UmbEntityItemRefElement } from './entity-item-ref.element.js';
import './entity-item-ref.element.js';
import { expect, waitUntil } from '@open-wc/testing';

describe('UmbEntityItemRefElement', () => {
	let element: UmbEntityItemRefElement;

	beforeEach(async () => {
		element = document.createElement('umb-entity-item-ref');
		element.innerHTML = '<span slot="detail">Detail content</span><span slot="actions">Actions content</span>';
		document.body.appendChild(element);
		element.item = { unique: 'item-1', entityType: 'test-entity-type-without-ref', name: 'Item' } as never;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	const getRefNode = () =>
		element.shadowRoot?.querySelector('umb-default-item-ref')?.shadowRoot?.querySelector('uui-ref-node');

	const getAssignedText = (slotName: string) => {
		const slot = getRefNode()?.querySelector(`slot[name="${slotName}"]`) as HTMLSlotElement | null;
		return slot
			?.assignedElements({ flatten: true })
			.map((assigned) => assigned.textContent)
			.join('');
	};

	it('forwards the detail slot to the rendered ref', async () => {
		await waitUntil(() => getRefNode(), 'expected the default item ref to render');

		expect(getAssignedText('detail')).to.equal('Detail content');
	});

	it('forwards the actions slot to the rendered ref', async () => {
		await waitUntil(() => getRefNode(), 'expected the default item ref to render');

		expect(getAssignedText('actions')).to.equal('Actions content');
	});
});
