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
	public addChildControl(child: UmbTestFormControlElement) {
		this.addFormControlElement(child as any);
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

	function resetCounts() {
		validEvents = 0;
		invalidEvents = 0;
	}

	beforeEach(async () => {
		element = await fixture(html`<umb-test-form-control></umb-test-form-control>`);
		resetCounts();
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
			// Establish a known baseline first: mounting the element already reports Valid once (nothing has
			// failed yet), and the de-duplication below (see 'does not repeat...') would otherwise silently
			// swallow a second, identical Valid report. Driving an Invalid report first makes the transition back
			// to Valid an actual change, so it's guaranteed to be dispatched regardless of that de-duplication.
			element.failCustomValidation('Required');
			await setPristine(false);
			resetCounts();

			element.passCustomValidation();

			expect(element.validity.valid).to.be.true;
			expect(validEvents).to.equal(1);
		});
	});

	describe('going pristine again while still failing validation', () => {
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
			resetCounts();

			await setPristine(true);

			expect(element.validity.valid).to.be.false; // still genuinely invalid underneath.
			expect(validEvents).to.equal(1); // ...but a Valid event was still dispatched, to clear stale feedback.
		});
	});

	describe('going pristine again once already reporting Valid', () => {
		it('does not need to report Valid again — the earlier report already covered it', async () => {
			element.failCustomValidation('Required');
			await setPristine(false);
			expect(invalidEvents).to.equal(1);

			// Becoming actually valid while still non-pristine already reports Valid once — clearing any stale
			// feedback there and then, same as the "going non-pristine while no validator fails" case above.
			element.passCustomValidation();
			expect(validEvents).to.equal(1);
			resetCounts();

			// So going pristine again afterwards is correctly deduped: there is nothing new to tell listeners,
			// since they were already told Valid a moment ago.
			await setPristine(true);

			expect(element.validity.valid).to.be.true;
			expect(validEvents).to.equal(0);
		});
	});

	describe('repeated reports of the same validation state', () => {
		it('does not repeat the same event type on consecutive, unchanged reports', async () => {
			element.failCustomValidation('Required');
			await setPristine(false);
			expect(invalidEvents).to.equal(1);

			// Re-running validation without anything about the outcome changing (still non-pristine, still
			// invalid, same message) must not dispatch a second Invalid event.
			element.failCustomValidation('Required');

			expect(invalidEvents).to.equal(1);
		});

		it('does dispatch again once the state actually changes back', async () => {
			element.failCustomValidation('Required');
			await setPristine(false);
			expect(invalidEvents).to.equal(1);

			element.passCustomValidation();

			expect(validEvents).to.equal(1);
		});
	});

	describe('going non-pristine', () => {
		it('cascades pristine=false onto nested form control elements', async () => {
			const child = await fixture<UmbTestFormControlElement>(html`<umb-test-form-control></umb-test-form-control>`);
			element.addChildControl(child);
			expect(child.pristine).to.be.true;

			await setPristine(false);

			expect(child.pristine).to.be.false;
		});
	});

	describe('going pristine again', () => {
		it('does NOT cascade pristine=true onto nested form control elements', async () => {
			// Asymmetric by design: a nested control that has already been revealed as dirty/invalid must not be
			// silently hidden again just because its container was reset — only the container's own pristine state
			// is under this setter's control.
			const child = await fixture<UmbTestFormControlElement>(html`<umb-test-form-control></umb-test-form-control>`);
			element.addChildControl(child);
			await setPristine(false);
			expect(child.pristine).to.be.false;

			await setPristine(true);

			expect(child.pristine).to.be.false;
		});
	});
});
