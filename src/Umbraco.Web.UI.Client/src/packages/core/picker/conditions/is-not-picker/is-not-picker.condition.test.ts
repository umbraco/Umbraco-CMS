import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbIsNotPickerCondition } from './is-not-picker.condition.js';
import { UMB_IS_NOT_PICKER_CONDITION_ALIAS } from './constants.js';
import { UmbPickerContext } from '../../picker.context.js';

@customElement('test-controller-host-not-in-picker')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-host-with-picker-context-not-in-picker')
class UmbTestControllerHostWithPickerContextElement extends UmbControllerHostElementMixin(HTMLElement) {
	pickerContext = new UmbPickerContext(this);
}

describe('UmbIsNotPickerCondition', () => {
	describe('without picker context', () => {
		let hostElement: UmbTestControllerHostElement;
		let condition: UmbIsNotPickerCondition;

		beforeEach(async () => {
			hostElement = new UmbTestControllerHostElement();
			document.body.appendChild(hostElement);
		});

		afterEach(() => {
			document.body.innerHTML = '';
		});

		it('should return true when not inside a picker context', async () => {
			condition = new UmbIsNotPickerCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_NOT_PICKER_CONDITION_ALIAS,
				},
				onChange: () => {},
			});

			// Wait for context consumption to settle
			await new Promise((resolve) => requestAnimationFrame(resolve));

			// The condition defaults to true and should remain true since no picker context exists
			expect(condition.permitted).to.be.true;
			condition.destroy();
		});
	});

	describe('with picker context', () => {
		let hostElement: UmbTestControllerHostWithPickerContextElement;
		let condition: UmbIsNotPickerCondition | undefined;

		beforeEach(async () => {
			hostElement = new UmbTestControllerHostWithPickerContextElement();
			document.body.appendChild(hostElement);
		});

		afterEach(() => {
			document.body.innerHTML = '';
		});

		it('should return false when inside a picker context', (done) => {
			let resolved = false;
			condition = new UmbIsNotPickerCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_NOT_PICKER_CONDITION_ALIAS,
				},
				onChange: () => {
					// Skip if already resolved or if condition is not yet assigned (during constructor)
					if (resolved || !condition) return;
					// Only check when permitted becomes false (context was found)
					if (condition.permitted === false) {
						resolved = true;
						expect(condition.permitted).to.be.false;
						condition.destroy();
						done();
					}
				},
			});
		});
	});
});
