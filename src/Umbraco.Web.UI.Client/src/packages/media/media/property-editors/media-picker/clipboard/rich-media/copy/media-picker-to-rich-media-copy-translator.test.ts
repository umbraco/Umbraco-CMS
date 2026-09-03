import { UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator } from './media-picker-to-rich-media-copy-translator.js';
import type { UmbMediaPickerValueModel } from '../../../../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-media-picker-to-rich-media-copy-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator;

	const propertyValue: UmbMediaPickerValueModel = [
		{
			key: 'entryKey',
			mediaKey: 'media-1',
			mediaTypeAlias: 'Image',
			focalPoint: { left: 0.4, top: 0.6 },
			crops: [{ alias: 'square', width: 100, height: 100 }],
		},
	];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each entry to a rich media reference, dropping the item key and media type', async () => {
		const result = await copyTranslator.translate(propertyValue);
		expect(result).to.deep.equal([
			{
				unique: 'media-1',
				focalPoint: { left: 0.4, top: 0.6 },
				crops: [{ alias: 'square', width: 100, height: 100 }],
			},
		]);
	});

	it('defaults focal point to null and crops to an empty array', async () => {
		const result = await copyTranslator.translate([
			{ key: 'k', mediaKey: 'media-2', mediaTypeAlias: '', focalPoint: null, crops: [] },
		]);
		expect(result).to.deep.equal([{ unique: 'media-2', focalPoint: null, crops: [] }]);
	});

	it('throws when the property value is missing', async () => {
		let error: unknown;
		try {
			await copyTranslator.translate(undefined as never);
		} catch (e) {
			error = e;
		}
		expect(error).to.be.instanceOf(Error);
	});
});
