import { UmbPropertyEditorUIImageCropsElement } from './property-editor-ui-image-crops.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbPropertyEditorUIImageCropsElement', () => {
	let element: UmbPropertyEditorUIImageCropsElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-image-crops></umb-property-editor-ui-image-crops> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIImageCropsElement);
	});
});
