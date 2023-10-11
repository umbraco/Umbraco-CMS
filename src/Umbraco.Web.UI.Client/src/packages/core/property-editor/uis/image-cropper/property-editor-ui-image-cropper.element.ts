import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import '../../../components/input-image-cropper/input-image-cropper.element.js';

/**
 * @element umb-property-editor-ui-image-cropper
 */
@customElement('umb-property-editor-ui-image-cropper')
export class UmbPropertyEditorUIImageCropperElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#DEBUG_CROP = {
		focalPoint: { left: 0.5, top: 0.5 },
		src: 'src/assets/TEST 4.png',
		crops: [
			{
				alias: 'Almost Bot Left',
				width: 1000,
				height: 1000,
				coordinates: {
					x1: 0.04113924050632909,
					x2: 0.3120537974683548,
					y1: 0.32154746835443077,
					y2: 0.031645569620253146,
				},
			},
			{
				alias: 'Almost top right',
				width: 1000,
				height: 1000,
				coordinates: {
					x1: 0.3086962025316458,
					x2: 0.04449683544303807,
					y1: 0.04746835443037985,
					y2: 0.305724683544304,
				},
			},
			{
				alias: 'TopLeft',
				width: 1000,
				height: 1000,
				coordinates: {
					x1: 0,
					x2: 0.5,
					y1: 0,
					y2: 0.5,
				},
			},
			{
				alias: 'bottomRight',
				width: 1000,
				height: 1000,
				coordinates: {
					x1: 0.5,
					x2: 0,
					y1: 0.5,
					y2: 0,
				},
			},
			{
				alias: 'Gigantic crop',
				width: 40200,
				height: 104000,
			},
			{
				alias: 'Desktop',
				width: 1920,
				height: 1080,
			},
			{
				alias: 'Banner',
				width: 1920,
				height: 300,
			},
			{
				alias: 'Tablet',
				width: 600,
				height: 800,
			},
			{
				alias: 'Mobile',
				width: 400,
				height: 800,
			},
		],
	};

	render() {
		return html`<umb-input-image-cropper .value=${this.#DEBUG_CROP}></umb-input-image-cropper>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
