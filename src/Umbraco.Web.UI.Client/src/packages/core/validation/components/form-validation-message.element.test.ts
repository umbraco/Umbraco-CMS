import { UmbFormValidationMessageElement } from './form-validation-message.element.js';
import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbFormValidationMessageElement', () => {
	let element: UmbFormValidationMessageElement;
	let control: HTMLElement & { pristine: boolean; validationMessage: string };

	beforeEach(async () => {
		element = await fixture(html`<umb-form-validation-message><input /></umb-form-validation-message>`);
		control = Object.assign(element.querySelector('input')!, { pristine: false });
	});

	async function reportInvalid(message: string) {
		Object.defineProperty(control, 'validationMessage', { value: message, configurable: true });
		control.dispatchEvent(new UmbValidationInvalidEvent());
		await element.updateComplete;
	}

	function messages(): HTMLElement {
		return element.shadowRoot!.querySelector('#messages')!;
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbFormValidationMessageElement);
	});

	it('shows the message of an invalid control and removes it once valid', async () => {
		await reportInvalid('This field is required');
		expect(messages().textContent).to.contain('This field is required');

		control.dispatchEvent(new UmbValidationValidEvent());
		await element.updateComplete;
		expect(messages().textContent).not.to.contain('This field is required');
	});

	it('keeps formatting markup in a message', async () => {
		await reportInvalid('Minimum 2 entries, requires <strong>1</strong> more.');
		expect(messages().querySelector('strong')?.textContent).to.equal('1');
	});

	it('renders a message through the HTML sanitizer', async () => {
		await reportInvalid('<strong onclick="void 0">Invalid</strong><script>void 0</script>');
		const strong = messages().querySelector('strong');
		expect(strong?.textContent).to.equal('Invalid');
		expect(strong?.hasAttribute('onclick')).to.be.false;
		expect(messages().querySelector('script')).to.be.null;
	});
});
