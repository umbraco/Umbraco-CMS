import {
	UmbPropertyEditorUIStaticImageFilePickerElement,
	isPickableImageFile,
} from './property-editor-ui-static-image-file-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIStaticImageFilePickerElement', () => {
	let element: UmbPropertyEditorUIStaticImageFilePickerElement;

	beforeEach(async () => {
		element = await fixture(
			html` <umb-property-editor-ui-static-image-file-picker></umb-property-editor-ui-static-image-file-picker> `,
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIStaticImageFilePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('isPickableImageFile', () => {
		const imageFileTypes = ['jpg', 'jpeg', 'png', 'webp', 'svg'];

		it('never allows folders to be picked', () => {
			expect(isPickableImageFile('/wwwroot/images', true, imageFileTypes)).to.be.false;
			expect(isPickableImageFile('/wwwroot/images', true, undefined)).to.be.false;
		});

		it('allows files with an allowed image extension', () => {
			expect(isPickableImageFile('/wwwroot/images/logo.png', false, imageFileTypes)).to.be.true;
			expect(isPickableImageFile('/wwwroot/images/icon.svg', false, imageFileTypes)).to.be.true;
		});

		it('matches extensions case-insensitively', () => {
			expect(isPickableImageFile('/wwwroot/images/LOGO.PNG', false, imageFileTypes)).to.be.true;
		});

		it('rejects files with a non-image extension', () => {
			expect(isPickableImageFile('/wwwroot/css/site.css', false, imageFileTypes)).to.be.false;
			expect(isPickableImageFile('/wwwroot/js/app.js', false, imageFileTypes)).to.be.false;
		});

		it('rejects files without an extension', () => {
			expect(isPickableImageFile('/wwwroot/README', false, imageFileTypes)).to.be.false;
		});

		it('allows any file while the configuration has not loaded yet', () => {
			expect(isPickableImageFile('/wwwroot/css/site.css', false, undefined)).to.be.true;
			expect(isPickableImageFile('/wwwroot/css/site.css', false, [])).to.be.true;
		});
	});
});
