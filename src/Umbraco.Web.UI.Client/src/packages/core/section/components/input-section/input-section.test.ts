//TODO: Test has been commented out while we figure out how to setup import maps for the test environment
// import { UmbPickerSectionElement } from './picker-section.element.js';
// import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// describe('UmbPickerSectionElement', () => {
// 	let element: UmbPickerSectionElement;
// 	beforeEach(async () => {
// 		element = await fixture(html`<umb-input-section></umb-input-section>`);
// 	});

// 	it('is defined with its own instance', () => {
// 		expect(element).to.be.instanceOf(UmbPickerSectionElement);
// 	});

// if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
// 	it('passes the a11y audit', async () => {
// 		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
// 	});
// }
// });
