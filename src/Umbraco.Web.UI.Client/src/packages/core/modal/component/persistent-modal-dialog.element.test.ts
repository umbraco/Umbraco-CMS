import { UmbPersistentModalDialogElement } from './persistent-modal-dialog.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UUIModalDialogElement } from '@umbraco-cms/backoffice/external/uui';

describe('UmbPersistentModalDialogElement', () => {
	let element: UmbPersistentModalDialogElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-persistent-modal-dialog></umb-persistent-modal-dialog>`);
		// `_openModal` defers `isOpen = true` via queueMicrotask
		await aTimeout(0);
	});

	afterEach(() => {
		if (!element.isClosing) {
			element.forceClose();
		}
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPersistentModalDialogElement);
	});

	it('extends UUIModalDialogElement', () => {
		expect(element).to.be.instanceOf(UUIModalDialogElement);
	});

	it('auto-opens the dialog after first update', () => {
		expect(element.isOpen).to.equal(true);
		const dialog = element.shadowRoot?.querySelector('dialog');
		expect(dialog?.open).to.equal(true);
	});

	it('prevents the Escape key from closing the dialog', () => {
		const dialog = element.shadowRoot?.querySelector('dialog') as HTMLDialogElement;
		const keydown = new KeyboardEvent('keydown', { key: 'Escape', cancelable: true, bubbles: true });
		dialog.dispatchEvent(keydown);

		expect(keydown.defaultPrevented).to.equal(true);
		expect(element.isOpen).to.equal(true);
	});

	it('does not interfere with non-Escape keys', () => {
		const dialog = element.shadowRoot?.querySelector('dialog') as HTMLDialogElement;
		const keydown = new KeyboardEvent('keydown', { key: 'Enter', cancelable: true, bubbles: true });
		dialog.dispatchEvent(keydown);

		expect(keydown.defaultPrevented).to.equal(false);
	});

	it('prevents the dialog cancel event from dismissing the modal', () => {
		const dialog = element.shadowRoot?.querySelector('dialog') as HTMLDialogElement;
		const cancelEvent = new Event('cancel', { cancelable: true });
		dialog.dispatchEvent(cancelEvent);

		expect(cancelEvent.defaultPrevented).to.equal(true);
		expect(element.isOpen).to.equal(true);
	});

	it('closes when forceClose is called', () => {
		element.forceClose();
		expect(element.isOpen).to.equal(false);
		expect(element.isClosing).to.equal(true);
	});

	it('removes the listeners after forceClose', () => {
		const dialog = element.shadowRoot?.querySelector('dialog') as HTMLDialogElement;
		element.forceClose();

		const keydown = new KeyboardEvent('keydown', { key: 'Escape', cancelable: true });
		dialog.dispatchEvent(keydown);
		expect(keydown.defaultPrevented).to.equal(false);

		const cancelEvent = new Event('cancel', { cancelable: true });
		dialog.dispatchEvent(cancelEvent);
		expect(cancelEvent.defaultPrevented).to.equal(false);
	});
});
