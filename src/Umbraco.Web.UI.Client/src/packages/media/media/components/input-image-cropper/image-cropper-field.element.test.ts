import { UmbInputImageCropperFieldElement } from './image-cropper-field.element.js';
import type { UmbImageCropperElement } from './image-cropper.element.js';
import type { UmbImageCropperPreviewElement } from './image-cropper-preview.element.js';
import type { UmbImageCropperPropertyEditorValue } from './types.js';
import './image-cropper.element.js';
import './image-cropper-preview.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { sendKeys } from '@web/test-runner-commands';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

const value: UmbImageCropperPropertyEditorValue = {
	src: '',
	focalPoint: { left: 0.5, top: 0.5 },
	crops: [
		{ alias: 'banner', label: 'Hero banner', width: 1200, height: 400 },
		{ alias: 'thumbnail', width: 300, height: 300 },
	],
};

describe('UmbInputImageCropperFieldElement', () => {
	let element: UmbInputImageCropperFieldElement;

	async function getPreview(alias: string) {
		await element.updateComplete;
		const previews = element.shadowRoot!.querySelectorAll<UmbImageCropperPreviewElement>('umb-image-cropper-preview');
		const preview = Array.from(previews).find((x) => x.crop?.alias === alias);
		expect(preview, `preview for crop "${alias}"`).to.exist;
		await preview!.updateComplete;
		return preview!;
	}

	function getButton(preview: UmbImageCropperPreviewElement) {
		return preview.shadowRoot!.querySelector<HTMLButtonElement>('#button')!;
	}

	async function getCropper() {
		await element.updateComplete;
		const cropper = element.shadowRoot!.querySelector<UmbImageCropperElement>('umb-image-cropper');
		expect(cropper, 'cropper').to.exist;
		await cropper!.updateComplete;
		return cropper!;
	}

	async function activateWithKey(alias: string, key: 'Enter' | 'Space') {
		getButton(await getPreview(alias)).focus();
		await sendKeys({ press: key });
		await element.updateComplete;
	}

	beforeEach(async () => {
		element = await fixture(html`<umb-image-cropper-field .value=${value}></umb-image-cropper-field>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputImageCropperFieldElement);
	});

	it('renders each crop as a native button named by its visible text', async () => {
		const preview = await getPreview('banner');
		const button = getButton(preview);

		expect(button.localName).to.equal('button');
		expect(button.type).to.equal('button');
		expect(button.hasAttribute('aria-label')).to.be.false;
		expect(preview.hasAttribute('aria-label')).to.be.false;
		expect(button.textContent).to.contain('Hero banner');
		expect(button.textContent).to.contain('1200 x 400');
	});

	it('selects the focused crop when Enter is pressed', async () => {
		await activateWithKey('banner', 'Enter');

		const preview = await getPreview('banner');
		expect(element.currentCrop?.alias).to.equal('banner');
		expect(preview.active).to.be.true;
		expect(getButton(preview).getAttribute('aria-current')).to.equal('true');
	});

	it('selects the focused crop when Space is pressed', async () => {
		await activateWithKey('thumbnail', 'Space');

		const preview = await getPreview('thumbnail');
		expect(element.currentCrop?.alias).to.equal('thumbnail');
		expect(preview.active).to.be.true;
		expect(getButton(preview).getAttribute('aria-current')).to.equal('true');
	});

	it('only marks the crop being edited as current', async () => {
		await activateWithKey('banner', 'Enter');

		const other = await getPreview('thumbnail');
		expect(other.active).to.be.false;
		expect(getButton(other).hasAttribute('aria-current')).to.be.false;
	});

	it('moves focus to the zoom slider when a crop is activated', async () => {
		await activateWithKey('banner', 'Enter');
		const cropper = await getCropper();

		await waitUntil(
			() => cropper.shadowRoot!.activeElement?.id === 'slider',
			'Expected focus to move to the zoom slider',
		);
	});

	it('gives the zoom slider an accessible name', async () => {
		await activateWithKey('banner', 'Enter');
		const cropper = await getCropper();

		const slider = cropper.shadowRoot!.querySelector('#slider')!;
		// Translations are not loaded in unit tests, so compare against the term as the cropper resolves it.
		expect(slider.getAttribute('label')).to.equal(cropper.localize.term('imagecropper_zoom'));
		expect(slider.getAttribute('label')).to.not.be.empty;
	});

	it('returns focus to the crop tile after Cancel', async () => {
		await activateWithKey('banner', 'Enter');
		const cropper = await getCropper();

		cropper.shadowRoot!.querySelector<HTMLElement>('uui-button[look="secondary"]')!.click();

		await waitUntil(() => !element.currentCrop, 'Expected the cropper to close');
		const preview = await getPreview('banner');
		await waitUntil(() => element.shadowRoot!.activeElement === preview, 'Expected focus on the crop tile');
		expect(preview.shadowRoot!.activeElement).to.equal(getButton(preview));
	});

	it('returns focus to the crop tile after Save', async () => {
		await activateWithKey('banner', 'Enter');
		const cropper = await getCropper();

		cropper.shadowRoot!.querySelector<HTMLElement>('uui-button[look="primary"]')!.click();

		await waitUntil(() => !element.currentCrop, 'Expected the cropper to close');
		// The tile is re-rendered after Save, so look it up again.
		const preview = await getPreview('banner');
		await waitUntil(() => element.shadowRoot!.activeElement === preview, 'Expected focus on the crop tile');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
