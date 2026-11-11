import { UmbEntityDetailForbiddenElement } from './entity-detail-forbidden.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('test-modal-host-entity-detail-forbidden')
class UmbTestModalHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntityDetailForbiddenElement', () => {
	let host: UmbTestModalHostElement;
	let element: UmbEntityDetailForbiddenElement;

	beforeEach(async () => {
		host = new UmbTestModalHostElement();
		element = new UmbEntityDetailForbiddenElement();
		host.appendChild(element);
		document.body.appendChild(host);
		await element.updateComplete;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEntityDetailForbiddenElement);
	});

	it('shows the default access denied text', () => {
		const text = element.shadowRoot?.textContent ?? '';
		expect(text).to.contain('entityDetail_forbiddenTitle');
		expect(text).to.contain('entityDetail_forbiddenDescription');
	});

	it('shows the headline and message instead of the default text when a headline is set', async () => {
		element.headline = 'Custom headline';
		element.message = 'Custom message';
		await element.updateComplete;

		const text = element.shadowRoot?.textContent ?? '';
		expect(text).to.contain('Custom headline');
		expect(text).to.contain('Custom message');
		expect(text).to.not.contain('entityDetail_forbiddenTitle');
	});

	it('keeps the default text when only a message is set', async () => {
		element.message = 'Custom message';
		await element.updateComplete;

		const text = element.shadowRoot?.textContent ?? '';
		expect(text).to.contain('entityDetail_forbiddenTitle');
		expect(text).to.not.contain('Custom message');
	});

	it('does not render a close button when not shown inside a modal', () => {
		const button = element.shadowRoot?.querySelector('uui-button');
		expect(button).to.be.null;
	});

	describe('when shown inside a modal', () => {
		let rejectCalled: boolean;

		beforeEach(async () => {
			rejectCalled = false;
			const mockModalContext = {
				getHostElement: () => host,
				reject: () => {
					rejectCalled = true;
				},
			} as unknown as UmbModalContext;

			const provider = new UmbContextProvider(host, UMB_MODAL_CONTEXT, mockModalContext);
			provider.hostConnected();

			await aTimeout(0);
			await element.updateComplete;
		});

		it('renders a close button', () => {
			const button = element.shadowRoot?.querySelector('uui-button');
			expect(button).to.not.be.null;
		});

		it('rejects the modal when the close button is clicked', () => {
			const button = element.shadowRoot?.querySelector('uui-button') as HTMLElement;
			button.dispatchEvent(new MouseEvent('click', { bubbles: true, composed: true }));
			expect(rejectCalled).to.be.true;
		});
	});
});
