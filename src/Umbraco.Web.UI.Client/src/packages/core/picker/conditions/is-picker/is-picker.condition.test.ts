import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbIsPickerCondition } from './is-picker.condition.js';
import { UMB_IS_PICKER_CONDITION_ALIAS } from './constants.js';
import { UmbPickerContext } from '../../picker.context.js';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-host-with-picker-context')
class UmbTestControllerHostWithPickerContextElement extends UmbControllerHostElementMixin(HTMLElement) {
	pickerContext = new UmbPickerContext(this);
}

describe('UmbIsPickerCondition', () => {
	describe('without picker context', () => {
		let hostElement: UmbTestControllerHostElement;
		let condition: UmbIsPickerCondition;

		beforeEach(async () => {
			hostElement = new UmbTestControllerHostElement();
			document.body.appendChild(hostElement);
		});

		afterEach(() => {
			document.body.innerHTML = '';
		});

		it('should return false when not inside a picker context', async () => {
			condition = new UmbIsPickerCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_PICKER_CONDITION_ALIAS,
				},
				onChange: () => {},
			});

			// Wait for context consumption to settle
			await new Promise((resolve) => requestAnimationFrame(resolve));

			expect(condition.permitted).to.be.false;
			condition.destroy();
		});
	});

	describe('with picker context', () => {
		let hostElement: UmbTestControllerHostWithPickerContextElement;
		let condition: UmbIsPickerCondition;

		beforeEach(async () => {
			hostElement = new UmbTestControllerHostWithPickerContextElement();
			document.body.appendChild(hostElement);
		});

		afterEach(() => {
			document.body.innerHTML = '';
		});

		it('should return true when inside a picker context', (done) => {
			let resolved = false;
			condition = new UmbIsPickerCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_IS_PICKER_CONDITION_ALIAS,
				},
				onChange: () => {
					if (resolved) return;
					resolved = true;
					expect(condition.permitted).to.be.true;
					condition.destroy();
					done();
				},
			});
		});
	});
});
