import type { UmbDropzoneMediaTypePickerModalElement } from './dropzone-media-type-picker-modal.element.js';
import type {
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue,
} from './dropzone-media-type-picker-modal.token.js';
import './dropzone-media-type-picker-modal.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

const IMAGE_TYPE = {
	unique: 'image',
	entityType: 'media-type',
	name: 'Image',
	description: null,
	icon: 'icon-picture',
};
const PHOTO_TYPE = {
	unique: 'photo',
	entityType: 'media-type',
	name: 'Photo',
	description: null,
	icon: 'icon-picture',
};

describe('UmbDropzoneMediaTypePickerModalElement', () => {
	let element: UmbDropzoneMediaTypePickerModalElement;
	let submitted: UmbDropzoneMediaTypePickerModalValue | undefined;
	let rejected: boolean;

	const render = async (data: Partial<UmbDropzoneMediaTypePickerModalData> = {}) => {
		submitted = undefined;
		rejected = false;
		element = document.createElement('umb-dropzone-media-type-picker-modal');
		element.modalContext = {
			value: new UmbObjectState(undefined).asObservable(),
			setValue: (value: UmbDropzoneMediaTypePickerModalValue) => (submitted = value),
			submit: () => {},
			reject: () => (rejected = true),
		} as any;
		element.data = { options: [IMAGE_TYPE, PHOTO_TYPE], ...data };
		document.body.appendChild(element);
		await element.updateComplete;
	};

	const click = async (selector: string) => {
		const target = element.shadowRoot!.querySelector<HTMLElement>(selector);
		expect(target, selector).to.exist;
		target!.click();
		await aTimeout(0);
	};

	afterEach(() => element.remove());

	it('names the files the choice is for', async () => {
		await render({ files: [new File(['a'], 'a.png'), new File(['b'], 'b.png')] });

		expect(element.shadowRoot!.querySelector('#files')!.textContent).to.match(/^a\.png,? and b\.png$/);
	});

	it('names the first few files and counts the rest', async () => {
		await render({ files: ['a', 'b', 'c', 'd', 'e'].map((name) => new File([name], `${name}.png`)) });

		// No dictionary is loaded in tests, so the remaining count shows as its key.
		expect(element.shadowRoot!.querySelector('#files')!.textContent).to.match(
			/^a\.png, b\.png, c\.png,? and media_chooseMediaTypeMoreFiles$/,
		);
	});

	it('picks a media type with a single click', async () => {
		await render();
		await click(`uui-button[label="${PHOTO_TYPE.name}"]`);

		expect(submitted).to.deep.equal({ mediaTypeUnique: PHOTO_TYPE.unique });
	});

	it('cancels without picking', async () => {
		await render();
		await click('#cancel');

		expect(rejected).to.be.true;
		expect(submitted).to.be.undefined;
	});
});
