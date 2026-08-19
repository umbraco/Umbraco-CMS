import { UmbFormControlValidator } from './form-control-validator.controller.js';
import { UmbValidationContext } from '../context/validation.context.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import type { UmbFormControlMixinInterface } from '../mixins/form-control.mixin.js';
import { expect, fixture } from '@open-wc/testing';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-form-control-validator-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestFormControlValidatorHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestConsumerController extends UmbControllerBase {}

/**
 * A minimal, standalone stand-in for *any* `UmbFormControlMixinInterface` implementation — deliberately not built
 * on `UmbFormControlMixin` itself, so these tests exercise only the contract between `UmbFormControlValidator`
 * and an arbitrary form control, not any behaviour specific to that one mixin.
 *
 * `checkValidity()` mirrors the one guarantee real form controls are expected to provide: it re-validates and
 * reports whatever it currently, synchronously holds — which is exactly what makes it unsafe to call against a
 * control whose value hasn't caught up to a just-changed dataPath yet.
 */
@customElement('umb-test-form-control-validator-control')
class UmbTestFormControlElement extends HTMLElement implements UmbFormControlMixinInterface<unknown> {
	pristine = true;
	value: unknown;
	validationMessage = '';

	#valid = true;

	setValid(valid: boolean, message = ''): void {
		this.#valid = valid;
		this.validationMessage = message;
	}

	checkValidity(): boolean {
		this.pristine = false;
		this.dispatchEvent(this.#valid ? new UmbValidationValidEvent() : new UmbValidationInvalidEvent());
		return this.#valid;
	}

	get validity(): ValidityState {
		return { valid: this.#valid } as ValidityState;
	}

	setCustomValidity(): void {}
	focusFirstInvalidElement(): void {}
	formResetCallback(): void {}
	addValidator() {
		return { flagKey: 'customError', getMessageMethod: () => '', checkMethod: () => true, weight: 0 } as const;
	}
	removeValidator(): void {}
}

describe('UmbFormControlValidator', () => {
	let host: UmbControllerHostElement;
	let context: UmbValidationContext;
	let control: UmbTestFormControlElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-form-control-validator-host></umb-test-form-control-validator-host>`);
		context = new UmbValidationContext(host);
		// Confirm the context is actually resolvable via consumeContext before relying on it below. [NL]
		await new UmbTestConsumerController(host).consumeContext(UMB_VALIDATION_CONTEXT, () => {}).asPromise();
		control = document.createElement('umb-test-form-control-validator-control') as UmbTestFormControlElement;
	});

	afterEach(() => {
		host.destroy();
	});

	describe('seeding #isValid on construction', () => {
		it('is valid when the dataPath has no known message', async () => {
			const validator = new UmbFormControlValidator(host, control, 'A');
			await Promise.resolve();

			expect(validator.isValid).to.be.true;
			expect(control.pristine).to.be.true;
		});

		it('is invalid when the dataPath has a known message, revealing it by un-pristining the control', async () => {
			context.messages.addMessage('client', 'A', 'Value cannot be empty');

			const validator = new UmbFormControlValidator(host, control, 'A');
			await Promise.resolve();

			expect(validator.isValid).to.be.false;
			expect(control.pristine).to.be.false;
		});

		it('is invalid when the dataPath has a known message, even if the control itself already reports non-pristine and valid', async () => {
			// The dataPath's known message must win regardless of what the control itself currently, physically
			// holds — that state can be stale, left over from whatever dataPath it was previously bound to (see
			// 'rebinding to a new dataPath...' below).
			context.messages.addMessage('client', 'A', 'Value cannot be empty');
			control.pristine = false;
			control.setValid(true);

			const validator = new UmbFormControlValidator(host, control, 'A');
			await Promise.resolve();

			expect(validator.isValid).to.be.false;
		});
	});

	describe('seeding #isValid on construction, without a dataPath', () => {
		// A validator with no dataPath (e.g. a group/container-level validator) has no message store entry to look
		// up, so it falls back to the control's own validity state instead of defaulting to either extreme.
		it('is valid when the control itself already reports valid', async () => {
			control.setValid(true);

			const validator = new UmbFormControlValidator(host, control, undefined);
			await Promise.resolve();

			expect(validator.isValid).to.be.true;
			expect(control.pristine).to.be.true;
		});

		it('is invalid when the control itself already reports invalid', async () => {
			control.setValid(false, 'Value cannot be empty');

			const validator = new UmbFormControlValidator(host, control, undefined);
			await Promise.resolve();

			expect(validator.isValid).to.be.false;
			expect(control.pristine).to.be.false;
		});
	});
});
