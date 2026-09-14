import { UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator } from './media-picker-to-media-copy-translator.js';
import type { UmbMediaPickerValueModel } from '../../../../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-media-picker-to-media-copy-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let copyTranslator: UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator;

	const propertyValue: UmbMediaPickerValueModel = [
		{ key: 'k1', mediaKey: 'media-1', mediaTypeAlias: 'Image', focalPoint: { left: 0.5, top: 0.5 }, crops: [] },
		{ key: 'k2', mediaKey: 'media-2', mediaTypeAlias: '', focalPoint: null, crops: [] },
	];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		copyTranslator = new UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each entry to a bare media reference, dropping crops and focal point', async () => {
		const result = await copyTranslator.translate(propertyValue);
		expect(result).to.deep.equal([{ unique: 'media-1' }, { unique: 'media-2' }]);
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
