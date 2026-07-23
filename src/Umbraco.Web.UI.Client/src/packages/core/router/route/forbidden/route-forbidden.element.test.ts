import { UmbRouteForbiddenElement } from './route-forbidden.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('test-modal-host-route-forbidden')
class UmbTestModalHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbRouteForbiddenElement', () => {
	let host: UmbTestModalHostElement;
	let element: UmbRouteForbiddenElement;

	beforeEach(async () => {
		host = new UmbTestModalHostElement();
		element = new UmbRouteForbiddenElement();
		host.appendChild(element);
		document.body.appendChild(host);
		await element.updateComplete;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbRouteForbiddenElement);
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
			} as any;

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
