import { UmbEntityItemRefFrameMixin } from './entity-item-ref-frame.mixin.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-test-entity-item-ref-frame')
class UmbTestEntityItemRefFrameElement extends UmbEntityItemRefFrameMixin(UmbLitElement) {
	@property({ type: Boolean })
	navigable = false;

	protected override get isEntityFrameNavigable() {
		return this.navigable;
	}

	override render() {
		return this.renderEntityFrame('Test name');
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-test-entity-item-ref-frame': UmbTestEntityItemRefFrameElement;
	}
}

describe('UmbEntityItemRefFrameMixin', () => {
	let element: UmbTestEntityItemRefFrameElement;

	beforeEach(async () => {
		element = await fixture<UmbTestEntityItemRefFrameElement>(
			html`<umb-test-entity-item-ref-frame></umb-test-entity-item-ref-frame>`,
		);
	});

	it('renders no entity frame when not navigable', () => {
		const frame = element.shadowRoot!.querySelector('umb-entity-frame');
		expect(frame).to.equal(null);
	});

	it('renders a hidden entity frame with the name when navigable', async () => {
		element.navigable = true;
		await element.updateComplete;

		const frame = element.shadowRoot!.querySelector('umb-entity-frame');
		expect(frame).to.not.equal(null);
		expect(frame!.getAttribute('aria-hidden')).to.equal('true');
		expect(frame!.textContent?.trim()).to.contain('Test name');
	});
});
