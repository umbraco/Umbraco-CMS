import { UmbFormControlMixin } from './form-control.mixin.js';
import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-test-form-control')
class UmbTestFormControlElement extends UmbFormControlMixin(UmbLitElement) {
	public failCustomValidation(message: string) {
		this.setCustomValidity(message);
	}
	public passCustomValidation() {
		this.setCustomValidity(undefined);
	}
}

describe('UmbFormControlMixin', () => {
	let element: UmbTestFormControlElement;
	let validEvents: number;
	let invalidEvents: number;

	// `UmbValidationInvalidEvent.TYPE` is the string `'invalid'` — the same name as the browser's native `invalid`
	// event, which `ElementInternals.checkValidity()` fires by itself whenever the control is invalid. Driving
	// state through `pristine` directly (as the real `UmbFormControlValidator` does when rebinding to a dataPath)
	// exercises `#dispatchValidationState()` without also triggering that unrelated native dispatch. [NL]
	async function setPristine(value: boolean) {
		element.pristine = value;
		await element.updateComplete;
	}

	beforeEach(async () => {
		element = await fixture(html`<umb-test-form-control></umb-test-form-control>`);
		validEvents = 0;
		invalidEvents = 0;
		element.addEventListener(UmbValidationValidEvent.TYPE, () => validEvents++);
		element.addEventListener(UmbValidationInvalidEvent.TYPE, () => invalidEvents++);
	});

	describe('while pristine', () => {
		it('does not report Invalid, even when a validator fails', async () => {
			element.failCustomValidation('Required');
			await element.updateComplete;

			expect(element.pristine).to.be.true;
			expect(invalidEvents).to.equal(0);
		});
	});

	describe('going non-pristine while a validator fails', () => {
		it('reports Invalid', async () => {
			element.failCustomValidation('Required');

			await setPristine(false);

			expect(element.validity.valid).to.be.false;
			expect(invalidEvents).to.equal(1);
		});
	});

	describe('going non-pristine while no validator fails', () => {
		it('reports Valid', async () => {
			await setPristine(false);

			expect(element.validity.valid).to.be.true;
			expect(validEvents).to.equal(1);
		});
	});

	describe('going pristine again while still failing validation', () => {
		// Bad comment:
		// This is the general rule behind the fix: pristine means "no validation feedback should be visible for
		// this control right now" — regardless of whether it happens to still be invalid underneath. A caller that
		// resets `pristine` back to true (e.g. a validator rebinding to a dataPath that currently has no known
		// issue, such as a property control reused across a variant switch) must therefore see a Valid event, so
		// that anything which reacted to the earlier Invalid event (like a displayed validation message) is told
		// to clear itself — even though nothing has actually fixed the control's underlying, still-failing value.
		it('reports Valid, clearing any previously shown Invalid state', async () => {
			element.failCustomValidation('Required');
			await setPristine(false);
			expect(invalidEvents).to.equal(1);

			validEvents = 0;
			await setPristine(true);

			expect(element.validity.valid).to.be.false; // still genuinely invalid underneath.
			expect(validEvents).to.equal(1); // ...but a Valid event was still dispatched, to clear stale feedback.
		});
	});

	describe('going pristine again once actually valid', () => {
		it('reports Valid', async () => {
			element.failCustomValidation('Required');
			await setPristine(false);
			expect(invalidEvents).to.equal(1);

			element.passCustomValidation();
			validEvents = 0; // reset — passCustomValidation() itself already reports Valid while still non-pristine.

			await setPristine(true);

			expect(element.validity.valid).to.be.true;
			expect(validEvents).to.equal(1);
		});
	});
});
