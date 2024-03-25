import { UMB_FORM_CONTEXT } from './form.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbValidationManager } from '@umbraco-cms/backoffice/validation';

/**
 * @description The Form Context is used to handle form submission and validation.
 * @event submit - Fired when the form is submitted.
 */
export class UmbFormContext extends UmbContextBase<UmbFormContext> {
	#formElement?: HTMLFormElement;
	#validation: UmbValidationManager[] = [];

	constructor(host: UmbControllerHost) {
		super(host, UMB_FORM_CONTEXT);
	}

	/**
	 * Method to call in the implementation once the form element is available.
	 * @param element {HTMLFormElement | null} - The Form element to be used for this context.
	 */
	setFormElement(element: HTMLFormElement | null) {
		if (this.#formElement === element) return;
		if (this.#formElement) {
			this.#formElement.removeEventListener('submit', this.onSubmit);
			this.#formElement.removeEventListener('reset', this.onReset);
		}
		if (element) {
			this.#formElement = element;
			this.#formElement.addEventListener('submit', this.onSubmit);
			this.#formElement.addEventListener('reset', this.onReset);
		}
	}

	/**
	 * Define one or more validation managers to be used for this form.
	 * These will be called when the form is requested to be submitted.
	 * @param manager {UmbValidationManager} - A manager to be bound to this form.
	 */
	registerValidationManager(manager: UmbValidationManager) {
		this.#validation.push(manager);
	}

	/**
	 * Call this method to submit the form
	 */
	requestSubmit() {
		// We do not call requestSubmit here, as we want the form to submit, and then we will handle the validation as part of the submit event handling.
		this.#formElement?.submit();
	}

	/**
	 * @description Triggered by the form, when it fires a submit event
	 */
	onSubmit = (event: SubmitEvent) => {
		event?.preventDefault();
		//this.dispatchEvent(new CustomEvent('submit-requested'));

		// Check client validation:
		const isClientValid = this.#formElement?.checkValidity();

		// ask validation managers to validate the form.

		const isValid = isClientValid ?? false;

		if (!isValid) {
			// Fire invalid..
			return;
		}

		// Fire submit event...
		this.dispatchEvent(new CustomEvent('submit'));
	};

	/**
	 * @description Triggered by the form, when it fires a reset event
	 */
	onReset = (event: Event) => {
		// ask validation managers to reset.
	};
}
